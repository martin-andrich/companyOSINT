using System.Security.Claims;
using System.Text.Encodings.Web;
using companyOSINT.Application.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace companyOSINT.Web.Authentication;

public class McpBearerAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IApiTokenService apiTokenService)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "McpBearer";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            return AuthenticateResult.NoResult();

        var headerValue = authHeader.ToString();
        if (!headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.NoResult();

        var plainToken = headerValue["Bearer ".Length..].Trim();
        if (string.IsNullOrEmpty(plainToken) || !plainToken.StartsWith("fac_"))
            return AuthenticateResult.Fail("Invalid token format.");

        var token = await apiTokenService.ValidateTokenAsync(plainToken, Context.RequestAborted);
        if (token is null)
            return AuthenticateResult.Fail("Invalid or expired token.");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, token.UserId.ToString()),
            new Claim("mcp:token_id", token.Id.ToString()),
            new Claim("mcp:token_name", token.Name),
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return AuthenticateResult.Success(ticket);
    }
}
