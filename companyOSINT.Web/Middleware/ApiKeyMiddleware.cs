namespace companyOSINT.Web.Middleware;

public class ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<ApiKeyMiddleware> logger)
{
    private const string ApiKeyHeaderName = "X-API-Key";

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            var configuredKey = configuration["ApiKey"];

            if (string.IsNullOrEmpty(configuredKey))
            {
                logger.LogError("ApiKey is not configured. Rejecting request to {Path}", context.Request.Path);
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("API key not configured on server");
                return;
            }

            if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var providedKey) ||
                !string.Equals(configuredKey, providedKey, StringComparison.Ordinal))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid or missing API key");
                return;
            }
        }

        await next(context);
    }
}
