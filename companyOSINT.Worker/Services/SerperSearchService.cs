using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text.Json;
using companyOSINT.Domain.Entities;
using companyOSINT.Worker.Models;

namespace companyOSINT.Worker.Services;

public class SerperSearchService(IHttpClientFactory httpClientFactory) : ISerperSearchService
{
    private const int AutoBlacklistThreshold = 25;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private volatile HashSet<string> _blacklistedDomains = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, int> _domainFrequency = new(StringComparer.OrdinalIgnoreCase);

    public void SetBlacklistedDomains(IEnumerable<string> domains)
    {
        _blacklistedDomains = new HashSet<string>(domains, StringComparer.OrdinalIgnoreCase);
    }

    public List<string> RecordDomains(List<SerperOrganicResult> results)
    {
        var promoted = new List<string>();

        foreach (var result in results)
        {
            var domain = ExtractDomain(result.Link);
            if (domain is null || _blacklistedDomains.Contains(domain))
                continue;

            var count = _domainFrequency.AddOrUpdate(domain, 1, (_, c) => c + 1);
            if (count == AutoBlacklistThreshold)
            {
                var updated = new HashSet<string>(_blacklistedDomains, StringComparer.OrdinalIgnoreCase) { domain };
                _blacklistedDomains = updated;
                promoted.Add(domain);
            }
        }

        return promoted;
    }

    private static string? ExtractDomain(string url)
    {
        try
        {
            var host = new Uri(url).Host;
            var dot = host.IndexOf('.');
            if (dot >= 0)
            {
                var parent = host[(dot + 1)..];
                if (parent.Contains('.'))
                    return parent;
            }
            return host;
        }
        catch
        {
            return null;
        }
    }

    public string BuildSearchQuery(Company company)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(company.Name))
            parts.Add($"\"{company.Name}\"");

        if (!string.IsNullOrWhiteSpace(company.RegisteredAddress))
            parts.Add(company.RegisteredAddress);

        return string.Join(" ", parts);
    }

    public async Task<List<SerperOrganicResult>> SearchAsync(string query, CancellationToken ct)
    {
        var serperClient = httpClientFactory.CreateClient("Serper");

        var requestBody = new { q = query, num = 10, gl = "de", hl = "de" };
        var response = await serperClient.PostAsJsonAsync("search", requestBody, ct);
        response.EnsureSuccessStatusCode();

        var serperResponse = await response.Content.ReadFromJsonAsync<SerperResponse>(JsonOptions, ct);
        return serperResponse?.Organic ?? [];
    }

    public bool IsBlacklistedUrl(string url)
    {
        try
        {
            var domains = _blacklistedDomains;
            var host = new Uri(url).Host;
            if (domains.Contains(host))
                return true;
            var dot = host.IndexOf('.');
            if (dot >= 0)
            {
                var parentDomain = host[(dot + 1)..];
                if (domains.Contains(parentDomain))
                    return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public bool IsPdfUrl(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return uri.AbsolutePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
        return url.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
    }
}
