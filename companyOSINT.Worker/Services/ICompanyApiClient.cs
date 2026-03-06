using companyOSINT.Domain.Common;
using companyOSINT.Domain.Entities;
using companyOSINT.Worker.Models;

namespace companyOSINT.Worker.Services;

public interface ICompanyApiClient
{
    Task<Company?> GetNextToCheckAsync(CancellationToken ct);
    Task PatchCompanyAsync(Guid companyId, Guid? sectorId, string activity, DateTime dateLastChecked, CancellationToken ct);
    Task CreateWebsiteAsync(Guid companyId, string urlWebsite, string? urlImprint, CancellationToken ct);
    IAsyncEnumerable<List<CompanyNameDto>> GetNamesToCheckBatchedAsync(CancellationToken ct);
    Task<List<string>> GetDomainsToSkipAsync(CancellationToken ct);
    Task CreateDomainToSkipAsync(string domain, CancellationToken ct);
    Task<Website?> GetNextWebsiteToEnrichAsync(CancellationToken ct);
    Task PatchWebsiteAsync(Guid websiteId, WebsitePatchRequest data, CancellationToken ct);
    Task ReplaceSoftwareAsync(Guid websiteId, List<SoftwareDetection> detections, CancellationToken ct);
    Task ReplaceToolsAsync(Guid websiteId, List<ToolDetection> detections, CancellationToken ct);
    Task<List<SectorDto>> GetSectorsAsync(CancellationToken ct);
    Task<SectorDto> CreateSectorAsync(string name, CancellationToken ct);
}
