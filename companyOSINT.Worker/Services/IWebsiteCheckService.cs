using companyOSINT.Worker.Models;

namespace companyOSINT.Worker.Services;

public interface IWebsiteCheckService
{
    Task<WebsiteCheckResult> CheckAsync(string url, CancellationToken ct);
}
