using companyOSINT.Domain.Dtos.Companies;
using companyOSINT.Application.Interfaces;
using companyOSINT.Domain.Common;
using companyOSINT.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace companyOSINT.Application.Services;

public class CompanyService(IApplicationDbContext db, IPostalCodeService postalCodeService) : ICompanyService
{
    public async Task<PaginatedResult<CompanyListDto>> GetAllAsync(
        string? name, string? federalState, string? registrar, string? search,
        Guid? sectorId, int page, int pageSize, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 100) pageSize = 25;

        var query = db.Companies.AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(c => c.Name != null && c.Name.Contains(name));

        if (!string.IsNullOrWhiteSpace(federalState))
            query = query.Where(c => c.FederalState == federalState);

        if (!string.IsNullOrWhiteSpace(registrar))
            query = query.Where(c => c.Registrar == registrar);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c =>
                (c.Name != null && c.Name.Contains(search)) ||
                (c.RegisteredAddress != null && c.RegisteredAddress.Contains(search)));

        if (sectorId.HasValue)
            query = query.Where(c => c.SectorId == sectorId);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CompanyListDto
            {
                Id = c.Id,
                Name = c.Name,
                FederalState = c.FederalState,
                RegisteredOffice = c.RegisteredOffice,
                Registrar = c.Registrar,
                RegisterArt = c.RegisterArt,
                RegisterNummer = c.RegisterNummer,
                Activity = c.Activity,
                SectorId = c.SectorId,
                SectorName = c.Sector != null ? c.Sector.Name : null,
                DateLastChecked = c.DateLastChecked,
            })
            .ToListAsync(ct);

        return new PaginatedResult<CompanyListDto>(items, totalCount, page, pageSize);
    }

    public async Task<Company?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Companies
            .Include(c => c.Sector)
            .Include(c => c.Contacts)
            .Include(c => c.Websites)
                .ThenInclude(w => w.Softwares)
            .Include(c => c.Websites)
                .ThenInclude(w => w.Tools)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<Company> CreateAsync(Company company, CancellationToken ct = default)
    {
        if (company.Id == Guid.Empty)
            company.Id = Guid.NewGuid();

        db.Companies.Add(company);
        await db.SaveChangesAsync(ct);

        return company;
    }

    public async Task<bool> UpdateAsync(Guid id, Company company, CancellationToken ct = default)
    {
        if (!await db.Companies.AnyAsync(c => c.Id == id, ct))
            return false;

        db.Companies.Entry(company).State = EntityState.Modified;
        await db.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> PatchAsync(Guid id, CompanyPatchDto dto, CancellationToken ct = default)
    {
        var company = await db.Companies.FindAsync([id], ct);

        if (company is null)
            return false;

        if (dto.SectorId.HasValue)
            company.SectorId = dto.SectorId;

        if (dto.Activity is not null)
            company.Activity = dto.Activity;

        if (dto.DateLastChecked.HasValue)
            company.DateLastChecked = dto.DateLastChecked;

        await db.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var company = await db.Companies.FindAsync([id], ct);

        if (company is null)
            return false;

        db.Companies.Remove(company);
        await db.SaveChangesAsync(ct);

        return true;
    }

    public async Task<Company?> GetNextToCheckAsync(CancellationToken ct = default)
    {
        return await db.Companies
            .FirstOrDefaultAsync(c => c.DateLastChecked == null && c.RegisteredOffice == "Dresden", ct);
    }

    public async Task<PaginatedResult<CompanySearchResultDto>> SearchAsync(
        string? postalCode, int radiusKm, List<Guid>? sectorIds,
        List<string>? softwareNames, List<string>? toolNames,
        bool? sslValid, bool? consentManagerFound, bool? hasWebsite,
        string? ttfbCategory, bool? hasRequestsWithoutConsent,
        int page, int pageSize, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 100) pageSize = 25;

        var query = db.Companies.AsQueryable();

        if (!string.IsNullOrWhiteSpace(postalCode))
        {
            var codes = postalCodeService.FindCodesWithinRadius(postalCode.Trim(), radiusKm);
            if (codes.Count > 0)
                query = query.Where(c => c.PostalCode != null && codes.Contains(c.PostalCode));
            else
                query = query.Where(c => false);
        }

        if (sectorIds is { Count: > 0 })
            query = query.Where(c => c.SectorId != null && sectorIds.Contains(c.SectorId.Value));

        if (softwareNames is { Count: > 0 })
            query = query.Where(c => c.Websites.Any(w => w.Softwares.Any(s => softwareNames.Contains(s.Name))));

        if (toolNames is { Count: > 0 })
            query = query.Where(c => c.Websites.Any(w => w.Tools.Any(t => toolNames.Contains(t.Name))));

        if (sslValid.HasValue)
            query = query.Where(c => c.Websites.Any(w => w.SslValid == sslValid.Value));

        if (consentManagerFound.HasValue)
            query = query.Where(c => c.Websites.Any(w => w.ConsentManagerFound == consentManagerFound.Value));

        if (hasWebsite == true)
            query = query.Where(c => c.Websites.Any());
        else if (hasWebsite == false)
            query = query.Where(c => !c.Websites.Any());

        if (ttfbCategory == "fast")
            query = query.Where(c => c.Websites.Any(w => w.AverageTimeToFirstByte > 0 && w.AverageTimeToFirstByte <= 800));
        else if (ttfbCategory == "medium")
            query = query.Where(c => c.Websites.Any(w => w.AverageTimeToFirstByte > 800 && w.AverageTimeToFirstByte <= 1800));
        else if (ttfbCategory == "slow")
            query = query.Where(c => c.Websites.Any(w => w.AverageTimeToFirstByte > 1800));

        if (hasRequestsWithoutConsent == true)
            query = query.Where(c => c.Websites.Any(w => w.RequestsWithoutConsent > 0));
        else if (hasRequestsWithoutConsent == false)
            query = query.Where(c => c.Websites.Any(w => w.RequestsWithoutConsent == 0));

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CompanySearchResultDto
            {
                Id = c.Id,
                Name = c.Name,
                RegisteredAddress = c.RegisteredAddress,
                Website = c.Websites.Select(w => w.UrlWebsite).FirstOrDefault(),
                Activity = c.Activity,
            })
            .ToListAsync(ct);

        return new PaginatedResult<CompanySearchResultDto>(items, totalCount, page, pageSize);
    }

    public async Task<List<string>> GetDistinctSoftwareNamesAsync(CancellationToken ct = default)
    {
        return await db.Software
            .Select(s => s.Name)
            .Distinct()
            .OrderBy(n => n)
            .ToListAsync(ct);
    }

    public async Task<List<string>> GetDistinctToolNamesAsync(CancellationToken ct = default)
    {
        return await db.Tools
            .Select(t => t.Name)
            .Distinct()
            .OrderBy(n => n)
            .ToListAsync(ct);
    }

    public async Task<CursorPage<CompanyNameDto>> GetNamesToCheckAsync(Guid? afterId, int pageSize, CancellationToken ct = default)
    {
        if (pageSize is < 1 or > 100_000) pageSize = 100_000;

        var query = db.Companies
            .Where(c => c.DateLastChecked == null);

        if (afterId.HasValue)
            query = query.Where(c => c.Id.CompareTo(afterId.Value) > 0);

        var items = await query
            .OrderBy(c => c.Id)
            .Take(pageSize + 1)
            .Select(c => new CompanyNameDto
            {
                Id = c.Id,
                Name = c.Name,
                RegisteredOffice = c.RegisteredOffice
            })
            .ToListAsync(ct);

        Guid? nextCursor = null;
        if (items.Count > pageSize)
        {
            items.RemoveAt(items.Count - 1);
            nextCursor = items[^1].Id;
        }

        return new CursorPage<CompanyNameDto>(items, nextCursor);
    }
}
