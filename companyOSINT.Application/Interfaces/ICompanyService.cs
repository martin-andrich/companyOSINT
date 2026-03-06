using companyOSINT.Domain.Dtos.Companies;
using companyOSINT.Domain.Common;
using companyOSINT.Domain.Entities;

namespace companyOSINT.Application.Interfaces;

public interface ICompanyService
{
    Task<PaginatedResult<CompanyListDto>> GetAllAsync(string? name, string? federalState, string? registrar, string? search, Guid? sectorId, int page, int pageSize, CancellationToken ct = default);
    Task<Company?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Company> CreateAsync(Company company, CancellationToken ct = default);
    Task<bool> UpdateAsync(Guid id, Company company, CancellationToken ct = default);
    Task<bool> PatchAsync(Guid id, CompanyPatchDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<Company?> GetNextToCheckAsync(CancellationToken ct = default);
    Task<CursorPage<CompanyNameDto>> GetNamesToCheckAsync(Guid? afterId, int pageSize, CancellationToken ct = default);
    Task<PaginatedResult<CompanySearchResultDto>> SearchAsync(string? postalCode, int radiusKm, List<Guid>? sectorIds, List<string>? softwareNames, List<string>? toolNames, bool? sslValid, bool? consentManagerFound, bool? hasWebsite, string? ttfbCategory, bool? hasRequestsWithoutConsent, int page, int pageSize, CancellationToken ct = default);
    Task<List<string>> GetDistinctSoftwareNamesAsync(CancellationToken ct = default);
    Task<List<string>> GetDistinctToolNamesAsync(CancellationToken ct = default);
}
