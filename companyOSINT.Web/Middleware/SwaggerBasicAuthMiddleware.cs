using System.Net.Http.Headers;
using System.Text;

namespace companyOSINT.Web.Middleware;

public class SwaggerBasicAuthMiddleware(RequestDelegate next, IConfiguration configuration)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            var username = configuration["Swagger:Username"]
                           ?? configuration["SWAGGER_USERNAME"];
            var password = configuration["Swagger:Password"]
                           ?? configuration["SWAGGER_PASSWORD"];

            // No credentials configured — allow open access
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                await next(context);
                return;
            }

            if (!context.Request.Headers.ContainsKey("Authorization"))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.Headers.WWWAuthenticate = "Basic realm=\"Swagger\"";
                return;
            }

            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(context.Request.Headers.Authorization!);
                var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader.Parameter!)).Split(':', 2);

                if (credentials.Length == 2 &&
                    string.Equals(credentials[0], username, StringComparison.Ordinal) &&
                    string.Equals(credentials[1], password, StringComparison.Ordinal))
                {
                    await next(context);
                    return;
                }
            }
            catch
            {
                // Invalid auth header format — fall through to 401
            }

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.Headers.WWWAuthenticate = "Basic realm=\"Swagger\"";
            return;
        }

        await next(context);
    }
}
