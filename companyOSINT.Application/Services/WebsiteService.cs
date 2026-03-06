using companyOSINT.Domain.Dtos.Websites;
using companyOSINT.Application.Interfaces;
using companyOSINT.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace companyOSINT.Application.Services;

public class WebsiteService(IApplicationDbContext db) : IWebsiteService
{
    public async Task<List<Website>> GetByCompanyAsync(Guid companyId, CancellationToken ct = default)
    {
        return await db.Websites
            .Include(w => w.Softwares)
            .Include(w => w.Tools)
            .Where(w => w.CompanyId == companyId)
            .ToListAsync(ct);
    }

    public async Task<Website?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Websites
            .Include(w => w.Softwares)
            .Include(w => w.Tools)
            .FirstOrDefaultAsync(w => w.Id == id, ct);
    }

    public async Task<(Website? Website, string? Error)> CreateAsync(WebsiteCreateDto dto, CancellationToken ct = default)
    {
        if (!await db.Companies.AnyAsync(c => c.Id == dto.CompanyId, ct))
            return (null, "Company not found");

        var website = new Website
        {
            Id = Guid.NewGuid(),
            CompanyId = dto.CompanyId,
            UrlWebsite = dto.UrlWebsite,
            UrlImprint = dto.UrlImprint,
        };

        db.Websites.Add(website);
        await db.SaveChangesAsync(ct);

        return (website, null);
    }

    public async Task<Website?> GetNextToEnrichAsync(CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-30);

        return await db.Websites
            .Where(w => w.UrlWebsite != null && w.UrlWebsite != "")
            .Where(w => w.DateLastChecked == null || w.DateLastChecked < cutoff)
            .OrderBy(w => w.DateLastChecked == null ? 0 : 1)
            .ThenBy(w => w.DateLastChecked)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> PatchAsync(Guid id, WebsitePatchDto dto, CancellationToken ct = default)
    {
        var website = await db.Websites.FindAsync([id], ct);

        if (website is null)
            return false;

        if (dto.HttpResponseCode.HasValue)
            website.HttpResponseCode = dto.HttpResponseCode.Value;
        if (dto.IpAddress is not null)
            website.IpAddress = dto.IpAddress;
        if (dto.SslValid.HasValue)
            website.SslValid = dto.SslValid.Value;
        if (dto.AverageTimeToFirstByte.HasValue)
            website.AverageTimeToFirstByte = dto.AverageTimeToFirstByte.Value;
        if (dto.DateLastChecked.HasValue)
            website.DateLastChecked = dto.DateLastChecked;
        if (dto.ConsentManagerFound.HasValue)
            website.ConsentManagerFound = dto.ConsentManagerFound.Value;
        if (dto.RequestsWithoutConsent.HasValue)
            website.RequestsWithoutConsent = dto.RequestsWithoutConsent.Value;
        if (dto.CookiesWithoutConsent.HasValue)
            website.CookiesWithoutConsent = dto.CookiesWithoutConsent.Value;

        await db.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var website = await db.Websites.FindAsync([id], ct);

        if (website is null)
            return false;

        db.Websites.Remove(website);
        await db.SaveChangesAsync(ct);

        return true;
    }

    public async Task<List<Software>> GetSoftwareAsync(Guid websiteId, CancellationToken ct = default)
    {
        return await db.Software
            .Where(s => s.WebsiteId == websiteId)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);
    }

    public async Task<(List<Software>? Software, string? Error)> ReplaceSoftwareAsync(Guid websiteId, List<SoftwareCreateDto> dtos, CancellationToken ct = default)
    {
        if (!await db.Websites.AnyAsync(w => w.Id == websiteId, ct))
            return (null, "Website not found");

        var existing = await db.Software.Where(s => s.WebsiteId == websiteId).ToListAsync(ct);
        db.Software.RemoveRange(existing);

        var newEntries = dtos.Select(dto => new Software
        {
            Id = Guid.NewGuid(),
            WebsiteId = websiteId,
            Name = dto.Name,
            Version = dto.Version,
            FoundAt = dto.FoundAt
        }).ToList();

        db.Software.AddRange(newEntries);
        await db.SaveChangesAsync(ct);

        return (newEntries, null);
    }

    public async Task<List<Tool>> GetToolsAsync(Guid websiteId, CancellationToken ct = default)
    {
        return await db.Tools
            .Where(t => t.WebsiteId == websiteId)
            .OrderBy(t => t.Name)
            .ToListAsync(ct);
    }

    public async Task<(List<Tool>? Tools, string? Error)> ReplaceToolsAsync(Guid websiteId, List<ToolCreateDto> dtos, CancellationToken ct = default)
    {
        if (!await db.Websites.AnyAsync(w => w.Id == websiteId, ct))
            return (null, "Website not found");

        var existing = await db.Tools.Where(t => t.WebsiteId == websiteId).ToListAsync(ct);
        db.Tools.RemoveRange(existing);

        var newEntries = dtos.Select(dto => new Tool
        {
            Id = Guid.NewGuid(),
            WebsiteId = websiteId,
            Name = dto.Name,
            FoundAt = dto.FoundAt
        }).ToList();

        db.Tools.AddRange(newEntries);
        await db.SaveChangesAsync(ct);

        return (newEntries, null);
    }
}
