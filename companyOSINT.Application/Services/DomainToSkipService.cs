using companyOSINT.Domain.Dtos.Domains;
using companyOSINT.Application.Interfaces;
using companyOSINT.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace companyOSINT.Application.Services;

public class DomainToSkipService(IApplicationDbContext db) : IDomainToSkipService
{
    public async Task<List<DomainToSkip>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.DomainsToSkip.OrderBy(d => d.Domain).ToListAsync(ct);
    }

    public async Task<DomainToSkip?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.DomainsToSkip.FindAsync([id], ct);
    }

    public async Task<(DomainToSkip? Domain, bool Conflict)> CreateAsync(DomainToSkipCreateDto dto, CancellationToken ct = default)
    {
        if (await db.DomainsToSkip.AnyAsync(d => d.Domain == dto.Domain, ct))
            return (null, true);

        var domain = new DomainToSkip
        {
            Id = Guid.NewGuid(),
            Domain = dto.Domain,
        };

        db.DomainsToSkip.Add(domain);
        await db.SaveChangesAsync(ct);

        return (domain, false);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var domain = await db.DomainsToSkip.FindAsync([id], ct);

        if (domain is null)
            return false;

        db.DomainsToSkip.Remove(domain);
        await db.SaveChangesAsync(ct);

        return true;
    }
}
