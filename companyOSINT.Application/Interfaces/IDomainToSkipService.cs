using companyOSINT.Domain.Dtos.Domains;
using companyOSINT.Domain.Entities;

namespace companyOSINT.Application.Interfaces;

public interface IDomainToSkipService
{
    Task<List<DomainToSkip>> GetAllAsync(CancellationToken ct = default);
    Task<DomainToSkip?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(DomainToSkip? Domain, bool Conflict)> CreateAsync(DomainToSkipCreateDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
