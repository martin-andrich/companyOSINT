using companyOSINT.Application;
using companyOSINT.Infrastructure;
using companyOSINT.Infrastructure.Data;
using companyOSINT.Web.Authentication;
using companyOSINT.Web.Components;
using companyOSINT.Web.Middleware;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

// Load .env file from solution root (if present) so dotnet run picks up env vars
var envFile = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".env");
if (File.Exists(envFile))
{
    foreach (var line in File.ReadAllLines(envFile))
    {
        var trimmed = line.Trim();
        if (trimmed.Length == 0 || trimmed.StartsWith('#'))
            continue;
        var sep = trimmed.IndexOf('=');
        if (sep <= 0)
            continue;
        var key = trimmed[..sep].Trim();
        var value = trimmed[(sep + 1)..].Trim();
        if (Environment.GetEnvironmentVariable(key) is null)
            Environment.SetEnvironmentVariable(key, value);
    }
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("DefaultConnection")!,
    builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

builder.Services.AddAuthentication()
    .AddScheme<AuthenticationSchemeOptions, McpBearerAuthenticationHandler>(
        McpBearerAuthenticationHandler.SchemeName, _ => { });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("McpPolicy", policy =>
    {
        policy.AuthenticationSchemes.Add(McpBearerAuthenticationHandler.SchemeName);
        policy.RequireAuthenticatedUser();
    });

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/de/anmelden";
    options.LogoutPath = "/de/abmelden";
    options.AccessDeniedPath = "/de/anmelden";
    options.Cookie.Name = "companyOSINT.Auth";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    await PostalCodeSeeder.SeedAsync(db);

    // Initialize postal code cache for radius search
    var postalCodeService = app.Services.GetRequiredService<companyOSINT.Application.Interfaces.IPostalCodeService>();
    await postalCodeService.InitializeCacheAsync();
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogWarning(ex, "Database migration failed. The application will start without applying migrations.");
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api") ||
        context.Request.Path.StartsWithSegments("/swagger") ||
        context.Request.Path.StartsWithSegments("/mcp") ||
        context.Request.Path.Equals("/de/impressum") ||
        context.Request.Path.Equals("/de/agb") ||
        context.Request.Path.Equals("/de/datenschutz"))
    {
        context.Response.Headers["X-Robots-Tag"] = "noindex, nofollow";
    }
    await next();
});

app.UseMiddleware<SwaggerBasicAuthMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

if (!app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        var host = context.Request.Host.Host;
        if (!string.Equals(host, "www.company-osint.com", StringComparison.OrdinalIgnoreCase))
        {
            var url = $"https://www.company-osint.com{context.Request.Path}{context.Request.QueryString}";
            context.Response.Redirect(url, permanent: true);
            return;
        }
        await next();
    });
}

app.MapGet("/", () => Results.Redirect("/de", permanent: true));

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapMcp("/mcp").RequireAuthorization("McpPolicy");
app.UseMiddleware<ApiKeyMiddleware>();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
