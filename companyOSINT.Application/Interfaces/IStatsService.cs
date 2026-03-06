namespace companyOSINT.Application.Interfaces;

public interface IStatsService
{
    Task<HomeStatsDto> GetHomeStatsAsync();
}

public record HomeStatsDto(int CompanyCount, int WebsiteCount, int SectorCount);
