using Microsoft.Extensions.Logging;

namespace companyOSINT.Worker.Services;

public class SectorCacheService(
    ICompanyApiClient apiClient,
    ILogger<SectorCacheService> logger) : ISectorCacheService
{
    private readonly SemaphoreSlim _lock = new(1, 1);

    private Dictionary<string, Guid> _nameToId = new(StringComparer.OrdinalIgnoreCase);
    private string _sectorListText = "";

    public async Task RefreshCacheAsync(CancellationToken ct)
    {
        await _lock.WaitAsync(ct);
        try
        {
            var sectors = await apiClient.GetSectorsAsync(ct);
            var nameToId = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

            foreach (var sector in sectors)
                nameToId[sector.Name] = sector.Id;

            var sectorListText = string.Join("\n", sectors
                .OrderBy(s => s.Name)
                .Select(s => $"- {s.Name}"));

            _nameToId = nameToId;
            _sectorListText = sectorListText;

            logger.LogInformation("Sector cache refreshed: {Count} sectors loaded", nameToId.Count);
        }
        finally
        {
            _lock.Release();
        }
    }

    public Guid? FindSectorId(string sectorName)
    {
        var index = _nameToId;
        return index.TryGetValue(sectorName, out var id) ? id : null;
    }

    public string GetSectorList()
    {
        return _sectorListText;
    }

    public async Task<Guid> GetOrCreateSectorAsync(string sectorName, CancellationToken ct)
    {
        var existingId = FindSectorId(sectorName);
        if (existingId.HasValue)
            return existingId.Value;

        var newSector = await apiClient.CreateSectorAsync(sectorName, ct);

        _nameToId[newSector.Name] = newSector.Id;

        logger.LogInformation("Created new sector: {Name} ({Id})", newSector.Name, newSector.Id);
        return newSector.Id;
    }
}
