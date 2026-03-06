using companyOSINT.Worker.Models;

namespace companyOSINT.Worker.Services;

public interface IConsentCheckService : IAsyncDisposable
{
    Task InitializeAsync(CancellationToken ct);
    Task<ConsentCheckResult> CheckAsync(string url, CancellationToken ct);
}
