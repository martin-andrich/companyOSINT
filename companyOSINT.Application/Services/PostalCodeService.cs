using companyOSINT.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace companyOSINT.Application.Services;

public class PostalCodeService(IServiceScopeFactory scopeFactory) : IPostalCodeService
{
    private const double EarthRadiusKm = 6371.0;

    private Dictionary<string, PostalCodeEntry>? _cache;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    public async Task InitializeCacheAsync(CancellationToken ct = default)
    {
        if (_cache is not null) return;

        await _initLock.WaitAsync(ct);
        try
        {
            if (_cache is not null) return;

            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

            var entries = await db.PostalCodes
                .AsNoTracking()
                .ToDictionaryAsync(
                    p => p.Code,
                    p => new PostalCodeEntry(p.Latitude, p.Longitude, p.Place),
                    ct);

            _cache = entries;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public List<string> FindCodesWithinRadius(string centerCode, int radiusKm)
    {
        if (_cache is null)
            throw new InvalidOperationException("Cache not initialized. Call InitializeCacheAsync first.");

        if (!_cache.TryGetValue(centerCode, out var center))
            return [];

        if (radiusKm <= 0)
            return [centerCode];

        var results = new List<string>();
        var centerLatRad = DegreesToRadians(center.Latitude);
        var centerLonRad = DegreesToRadians(center.Longitude);

        foreach (var (code, entry) in _cache)
        {
            var distance = HaversineDistance(
                centerLatRad, centerLonRad,
                DegreesToRadians(entry.Latitude), DegreesToRadians(entry.Longitude));

            if (distance <= radiusKm)
                results.Add(code);
        }

        return results;
    }

    public string? GetPlaceName(string code)
    {
        if (_cache is null)
            throw new InvalidOperationException("Cache not initialized. Call InitializeCacheAsync first.");

        return _cache.TryGetValue(code, out var entry) ? entry.Place : null;
    }

    public string? FindNearestCode(double latitude, double longitude)
    {
        if (_cache is null)
            throw new InvalidOperationException("Cache not initialized. Call InitializeCacheAsync first.");

        string? nearest = null;
        var minDistance = double.MaxValue;
        var latRad = DegreesToRadians(latitude);
        var lonRad = DegreesToRadians(longitude);

        foreach (var (code, entry) in _cache)
        {
            var distance = HaversineDistance(
                latRad, lonRad,
                DegreesToRadians(entry.Latitude), DegreesToRadians(entry.Longitude));

            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = code;
            }
        }

        return nearest;
    }

    private static double HaversineDistance(double lat1Rad, double lon1Rad, double lat2Rad, double lon2Rad)
    {
        var dLat = lat2Rad - lat1Rad;
        var dLon = lon2Rad - lon1Rad;

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(lat1Rad) * Math.Cos(lat2Rad)
              * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        return 2 * EarthRadiusKm * Math.Asin(Math.Sqrt(a));
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;

    private sealed record PostalCodeEntry(double Latitude, double Longitude, string Place);
}
