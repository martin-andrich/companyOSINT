using companyOSINT.Domain.Dtos.Websites;
using companyOSINT.Domain.Entities;

namespace companyOSINT.Application.Interfaces;

public interface IWebsiteService
{
    Task<List<Website>> GetByCompanyAsync(Guid companyId, CancellationToken ct = default);
    Task<Website?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(Website? Website, string? Error)> CreateAsync(WebsiteCreateDto dto, CancellationToken ct = default);
    Task<Website?> GetNextToEnrichAsync(CancellationToken ct = default);
    Task<bool> PatchAsync(Guid id, WebsitePatchDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<List<Software>> GetSoftwareAsync(Guid websiteId, CancellationToken ct = default);
    Task<(List<Software>? Software, string? Error)> ReplaceSoftwareAsync(Guid websiteId, List<SoftwareCreateDto> dtos, CancellationToken ct = default);
    Task<List<Tool>> GetToolsAsync(Guid websiteId, CancellationToken ct = default);
    Task<(List<Tool>? Tools, string? Error)> ReplaceToolsAsync(Guid websiteId, List<ToolCreateDto> dtos, CancellationToken ct = default);
}
