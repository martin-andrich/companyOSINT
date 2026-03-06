namespace companyOSINT.Application.Interfaces;

public interface IPostalCodeService
{
    Task InitializeCacheAsync(CancellationToken ct = default);
    List<string> FindCodesWithinRadius(string centerCode, int radiusKm);
    string? GetPlaceName(string code);
    string? FindNearestCode(double latitude, double longitude);
}
