using companyOSINT.Domain.Common;

namespace companyOSINT.Worker.Services;

public interface ICompanyMatchingService
{
    /// <summary>
    /// Load/refresh the company name cache from the API.
    /// </summary>
    Task RefreshCacheAsync(CancellationToken ct);

    /// <summary>
    /// Check if the Impressum text contains a match for any company in the cache.
    /// Returns the matching company, or null if no match found.
    /// </summary>
    CompanyMatchResult? FindMatchInImpressum(string impressumText);

    /// <summary>
    /// Check if a specific company name matches in the Impressum text
    /// (replaces the old static CheckImpressumMatch).
    /// </summary>
    bool CheckImpressumMatch(string impressumText, string companyName);

    /// <summary>
    /// Remove a company from the cache after it has been enriched.
    /// </summary>
    void RemoveFromCache(Guid companyId);
}

public record CompanyMatchResult(Guid CompanyId, string CompanyName);
