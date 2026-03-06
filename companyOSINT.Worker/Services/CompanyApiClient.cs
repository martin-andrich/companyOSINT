using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using companyOSINT.Domain.Common;
using companyOSINT.Domain.Entities;
using companyOSINT.Worker.Models;

namespace companyOSINT.Worker.Services;

file record DomainToSkipDto(string Domain);

public class CompanyApiClient(IHttpClientFactory httpClientFactory) : ICompanyApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<Company?> GetNextToCheckAsync(CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("Api");
        var response = await client.GetAsync("api/companies/next-to-check", ct);

        if (response.StatusCode == HttpStatusCode.NoContent)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Company>(JsonOptions, ct);
    }

    public async Task PatchCompanyAsync(Guid companyId, Guid? sectorId, string activity, DateTime dateLastChecked, CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("Api");
        var response = await client.PatchAsJsonAsync(
            $"api/companies/{companyId}",
            new { sectorId, activity, dateLastChecked },
            ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task CreateWebsiteAsync(Guid companyId, string urlWebsite, string? urlImprint, CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("Api");
        var response = await client.PostAsJsonAsync(
            "api/websites",
            new { companyId, urlWebsite, urlImprint },
            ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<string>> GetDomainsToSkipAsync(CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("Api");
        var response = await client.GetAsync("api/domainstoskip", ct);
        response.EnsureSuccessStatusCode();

        var domains = await response.Content.ReadFromJsonAsync<List<DomainToSkipDto>>(JsonOptions, ct);
        return domains?.Select(d => d.Domain).ToList() ?? [];
    }

    public async Task CreateDomainToSkipAsync(string domain, CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("Api");
        var response = await client.PostAsJsonAsync("api/domainstoskip", new { domain }, ct);
        // Ignore 409 Conflict (domain already exists)
        if (response.StatusCode != HttpStatusCode.Conflict)
            response.EnsureSuccessStatusCode();
    }

    public async Task<Website?> GetNextWebsiteToEnrichAsync(CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("Api");
        var response = await client.GetAsync("api/websites/next-to-enrich", ct);

        if (response.StatusCode == HttpStatusCode.NoContent)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Website>(JsonOptions, ct);
    }

    public async Task PatchWebsiteAsync(Guid websiteId, WebsitePatchRequest data, CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("Api");
        var response = await client.PatchAsJsonAsync(
            $"api/websites/{websiteId}",
            new
            {
                data.HttpResponseCode,
                data.IpAddress,
                data.SslValid,
                data.AverageTimeToFirstByte,
                data.DateLastChecked,
                data.ConsentManagerFound,
                data.RequestsWithoutConsent,
                data.CookiesWithoutConsent
            },
            ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task ReplaceSoftwareAsync(Guid websiteId, List<SoftwareDetection> detections, CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("Api");
        var payload = detections.Select(d => new { d.Name, d.Version, d.FoundAt }).ToList();
        var response = await client.PutAsJsonAsync($"api/websites/{websiteId}/software", payload, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task ReplaceToolsAsync(Guid websiteId, List<ToolDetection> detections, CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("Api");
        var payload = detections.Select(d => new { d.Name, d.FoundAt }).ToList();
        var response = await client.PutAsJsonAsync($"api/websites/{websiteId}/tools", payload, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<SectorDto>> GetSectorsAsync(CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("Api");
        var response = await client.GetAsync("api/sectors", ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<SectorDto>>(JsonOptions, ct) ?? [];
    }

    public async Task<SectorDto> CreateSectorAsync(string name, CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("Api");
        var response = await client.PostAsJsonAsync("api/sectors", new { name }, ct);

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var sectors = await GetSectorsAsync(ct);
            return sectors.First(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SectorDto>(JsonOptions, ct)
               ?? throw new InvalidOperationException("Failed to deserialize created sector");
    }

    public async IAsyncEnumerable<List<CompanyNameDto>> GetNamesToCheckBatchedAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("Api");
        Guid? afterId = null;

        while (true)
        {
            var url = "api/companies/names-to-check";
            if (afterId.HasValue)
                url += $"?afterId={afterId.Value}";

            var response = await client.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            var page = await response.Content.ReadFromJsonAsync<CursorPage<CompanyNameDto>>(JsonOptions, ct);
            if (page is null || page.Items.Count == 0)
                yield break;

            yield return page.Items;

            if (page.NextCursor is null)
                yield break;

            afterId = page.NextCursor;
        }
    }
}
