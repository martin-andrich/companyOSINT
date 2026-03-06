using companyOSINT.Domain.Dtos.Sectors;
using companyOSINT.Application.Interfaces;
using companyOSINT.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace companyOSINT.Application.Services;

public class SectorService(IApplicationDbContext db) : ISectorService
{
    public async Task<List<Sector>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.Sectors.OrderBy(s => s.Name).ToListAsync(ct);
    }

    public async Task<Sector?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Sectors.FindAsync([id], ct);
    }

    public async Task<(Sector? Sector, bool Conflict)> CreateAsync(SectorCreateDto dto, CancellationToken ct = default)
    {
        if (await db.Sectors.AnyAsync(s => s.Name == dto.Name, ct))
            return (null, true);

        var sector = new Sector
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
        };

        db.Sectors.Add(sector);
        await db.SaveChangesAsync(ct);

        return (sector, false);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var sector = await db.Sectors.FindAsync([id], ct);

        if (sector is null)
            return false;

        db.Sectors.Remove(sector);
        await db.SaveChangesAsync(ct);

        return true;
    }
}
