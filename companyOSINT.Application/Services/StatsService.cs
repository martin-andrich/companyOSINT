using companyOSINT.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace companyOSINT.Application.Services;

public class StatsService(IApplicationDbContext db) : IStatsService
{
    public async Task<HomeStatsDto> GetHomeStatsAsync()
    {
        var companyCount = await db.Companies.CountAsync(c => c.DateLastChecked != null);
        var websiteCount = await db.Websites.CountAsync();
        var sectorCount = await db.Sectors.CountAsync();

        return new HomeStatsDto(companyCount, websiteCount, sectorCount);
    }
}
