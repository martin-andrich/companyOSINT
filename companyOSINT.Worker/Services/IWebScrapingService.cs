using companyOSINT.Worker.Models;

namespace companyOSINT.Worker.Services;

public interface IWebScrapingService
{
    Task<FetchResult> FetchAndExtractTextAsync(string url, CancellationToken ct);
    Task<string?> FetchPlainTextAsync(string url, CancellationToken ct);
    string NormalizeToBaseUrl(string url);
}
