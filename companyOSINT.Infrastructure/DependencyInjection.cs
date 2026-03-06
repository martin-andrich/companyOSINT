using companyOSINT.Application.Interfaces;
using companyOSINT.Infrastructure.Data;
using companyOSINT.Infrastructure.Email;
using companyOSINT.Infrastructure.Identity;
using companyOSINT.Infrastructure.Turnstile;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace companyOSINT.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString,
                npgsql => npgsql.MigrationsAssembly("companyOSINT.Web")));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        services.Configure<SmtpSettings>(configuration.GetSection("Smtp"));
        services.AddTransient<IEmailSender<ApplicationUser>, SmtpEmailSender>();
        services.AddTransient<IContactEmailService, ContactEmailService>();

        services.Configure<TurnstileSettings>(configuration.GetSection("Turnstile"));
        services.AddHttpClient<ITurnstileValidationService, TurnstileValidationService>();

        return services;
    }
}
