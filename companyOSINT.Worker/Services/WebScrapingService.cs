using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using companyOSINT.Worker.Models;

namespace companyOSINT.Worker.Services;

public partial class WebScrapingService(IHttpClientFactory httpClientFactory, ILogger<WebScrapingService> logger) : IWebScrapingService
{
    public async Task<FetchResult> FetchAndExtractTextAsync(string url, CancellationToken ct)
    {
        var webClient = httpClientFactory.CreateClient("WebFetch");

        HttpResponseMessage response;
        try
        {
            response = await webClient.GetAsync(url, ct);
        }
        catch (HttpRequestException ex)
        {
            logger.LogInformation("Skipping {Url}: {Message}", url, ex.InnerException?.Message ?? ex.Message);
            return new FetchResult("", null);
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            logger.LogInformation("Skipping {Url}: request timed out", url);
            return new FetchResult("", null);
        }

        if (!response.IsSuccessStatusCode)
        {
            logger.LogInformation("Skipping {Url}: {StatusCode}", url, (int)response.StatusCode);
            return new FetchResult("", null);
        }

        var html = await ReadContentAsStringAsync(response, url, ct);
        if (html is null)
            return new FetchResult("", null);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var impressumUrl = FindImpressumUrl(doc, url);

        // Remove script, style, noscript nodes
        var nodesToRemove = doc.DocumentNode.SelectNodes("//script|//style|//noscript");
        if (nodesToRemove is not null)
        {
            foreach (var node in nodesToRemove)
                node.Remove();
        }

        var text = doc.DocumentNode.InnerText;
        text = System.Net.WebUtility.HtmlDecode(text);
        text = CollapseWhitespaceRegex().Replace(text, " ").Trim();

        if (text.Length > 8000)
            text = text[..8000];

        return new FetchResult(text, impressumUrl);
    }

    public async Task<string?> FetchPlainTextAsync(string url, CancellationToken ct)
    {
        var webClient = httpClientFactory.CreateClient("WebFetch");

        HttpResponseMessage response;
        try
        {
            response = await webClient.GetAsync(url, ct);
        }
        catch (HttpRequestException ex)
        {
            logger.LogInformation("Skipping {Url}: {Message}", url, ex.InnerException?.Message ?? ex.Message);
            return null;
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            logger.LogInformation("Skipping {Url}: request timed out", url);
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            logger.LogInformation("Skipping {Url}: {StatusCode}", url, (int)response.StatusCode);
            return null;
        }

        var html = await ReadContentAsStringAsync(response, url, ct);
        if (html is null)
            return null;

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var nodesToRemove = doc.DocumentNode.SelectNodes("//script|//style|//noscript");
        if (nodesToRemove is not null)
        {
            foreach (var node in nodesToRemove)
                node.Remove();
        }

        var text = doc.DocumentNode.InnerText;
        text = System.Net.WebUtility.HtmlDecode(text);
        text = CollapseWhitespaceRegex().Replace(text, " ").Trim();

        if (text.Length > 8000)
            text = text[..8000];

        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    public string NormalizeToBaseUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            return $"{uri.Scheme}://{uri.Host}";
        }
        catch
        {
            return url;
        }
    }

    private static string? FindImpressumUrl(HtmlDocument doc, string baseUrl)
    {
        var links = doc.DocumentNode.SelectNodes("//a[@href]");
        if (links is null)
            return null;

        foreach (var link in links)
        {
            var linkText = link.InnerText.Trim();
            var href = link.GetAttributeValue("href", "");

            if (string.IsNullOrWhiteSpace(href))
                continue;

            var isImprint = linkText.Contains("Impressum", StringComparison.OrdinalIgnoreCase)
                            || linkText.Contains("Imprint", StringComparison.OrdinalIgnoreCase)
                            || href.Contains("impressum", StringComparison.OrdinalIgnoreCase)
                            || href.Contains("imprint", StringComparison.OrdinalIgnoreCase);

            if (!isImprint)
                continue;

            try
            {
                var resolved = new Uri(new Uri(baseUrl), href);
                return resolved.AbsoluteUri;
            }
            catch
            {
                // malformed href, skip
            }
        }

        return null;
    }

    private async Task<string?> ReadContentAsStringAsync(HttpResponseMessage response, string url, CancellationToken ct)
    {
        try
        {
            return await response.Content.ReadAsStringAsync(ct);
        }
        catch (InvalidOperationException)
        {
            var charSet = response.Content.Headers.ContentType?.CharSet ?? "unknown";
            logger.LogInformation("Unsupported charset '{CharSet}' for {Url}, falling back to UTF-8", charSet, url);
            var bytes = await response.Content.ReadAsByteArrayAsync(ct);
            return Encoding.UTF8.GetString(bytes);
        }
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex CollapseWhitespaceRegex();
}
