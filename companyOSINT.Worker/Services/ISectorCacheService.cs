namespace companyOSINT.Worker.Services;

public interface ISectorCacheService
{
    Task RefreshCacheAsync(CancellationToken ct);
    Guid? FindSectorId(string sectorName);
    string GetSectorList();
    Task<Guid> GetOrCreateSectorAsync(string sectorName, CancellationToken ct);
}
