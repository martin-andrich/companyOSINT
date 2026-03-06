using companyOSINT.Domain.Entities;
using companyOSINT.Worker.Models;

namespace companyOSINT.Worker.Services;

public interface ISerperSearchService
{
    string BuildSearchQuery(Company company);
    Task<List<SerperOrganicResult>> SearchAsync(string query, CancellationToken ct);
    bool IsBlacklistedUrl(string url);
    bool IsPdfUrl(string url);
    void SetBlacklistedDomains(IEnumerable<string> domains);
    List<string> RecordDomains(List<SerperOrganicResult> results);
}
