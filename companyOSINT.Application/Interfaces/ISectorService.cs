using companyOSINT.Domain.Dtos.Sectors;
using companyOSINT.Domain.Entities;

namespace companyOSINT.Application.Interfaces;

public interface ISectorService
{
    Task<List<Sector>> GetAllAsync(CancellationToken ct = default);
    Task<Sector?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(Sector? Sector, bool Conflict)> CreateAsync(SectorCreateDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
