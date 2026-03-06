using companyOSINT.Application.Interfaces;
using companyOSINT.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace companyOSINT.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IWebsiteService, WebsiteService>();
        services.AddScoped<ISectorService, SectorService>();
        services.AddScoped<IDomainToSkipService, DomainToSkipService>();
        services.AddScoped<IUserProjectService, UserProjectService>();
        services.AddScoped<IApiTokenService, ApiTokenService>();
        services.AddSingleton<IPostalCodeService, PostalCodeService>();
        services.AddScoped<IStatsService, StatsService>();

        return services;
    }
}
