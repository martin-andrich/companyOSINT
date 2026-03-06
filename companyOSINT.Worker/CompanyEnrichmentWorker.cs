using companyOSINT.Domain.Entities;
using companyOSINT.Worker.Models;
using companyOSINT.Worker.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace companyOSINT.Worker;

public class CompanyEnrichmentWorker(
    ICompanyApiClient apiClient,
    ISerperSearchService serperService,
    IWebScrapingService webScrapingService,
    IOllamaService ollamaService,
    ICompanyMatchingService matchingService,
    ISectorCacheService sectorCacheService,
    IConfiguration configuration,
    ILogger<CompanyEnrichmentWorker> logger) : BackgroundService
{
    private const double ConfidenceThreshold = 0.6;
    private static readonly TimeSpan CacheRefreshInterval = TimeSpan.FromMinutes(60);
    private DateTime _lastCacheRefresh = DateTime.MinValue;
    private readonly bool _crossMatchingEnabled = configuration.GetValue("CrossMatchingEnabled", true);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("CompanyEnrichmentWorker started (cross-matching: {Enabled})", _crossMatchingEnabled);

        await LoadBlacklistedDomainsAsync(stoppingToken);
        await sectorCacheService.RefreshCacheAsync(stoppingToken);

        if (_crossMatchingEnabled)
        {
            await matchingService.RefreshCacheAsync(stoppingToken);
            _lastCacheRefresh = DateTime.UtcNow;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Periodic refresh of cache and blacklisted domains
                if (DateTime.UtcNow - _lastCacheRefresh > CacheRefreshInterval)
                {
                    await LoadBlacklistedDomainsAsync(stoppingToken);
                    await sectorCacheService.RefreshCacheAsync(stoppingToken);

                    if (_crossMatchingEnabled)
                        await matchingService.RefreshCacheAsync(stoppingToken);

                    _lastCacheRefresh = DateTime.UtcNow;
                }

                var company = await apiClient.GetNextToCheckAsync(stoppingToken);
                if (company is null)
                {
                    logger.LogInformation("No unchecked companies found, waiting 30s...");
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                    continue;
                }

                logger.LogInformation("Processing company {Id} ({Name})", company.Id, company.Name);

                EnrichmentResult result;
                try
                {
                    result = await EnrichCompanyAsync(company, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Enrichment failed for company {Id} ({Name}), marking as checked",
                        company.Id, company.Name);
                    result = new EnrichmentResult("", null, "");
                }

                logger.LogInformation(
                    "Company {Id} ({Name}): website={UrlWebsite}, sectorId={SectorId}, activity={Activity}",
                    company.Id, company.Name,
                    result.UrlWebsite == "" ? "(not found)" : result.UrlWebsite,
                    result.SectorId?.ToString() ?? "(unknown)",
                    result.Activity == "" ? "(unknown)" : result.Activity);

                // Create Website entity if a website was found
                if (!string.IsNullOrEmpty(result.UrlWebsite))
                {
                    await apiClient.CreateWebsiteAsync(company.Id, result.UrlWebsite, result.UrlImprint, stoppingToken);
                }

                // Patch company with sector, activity, and mark as checked
                await apiClient.PatchCompanyAsync(company.Id, result.SectorId, result.Activity, DateTime.UtcNow, stoppingToken);
                if (_crossMatchingEnabled)
                    matchingService.RemoveFromCache(company.Id);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing company, retrying in 10s...");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        logger.LogInformation("CompanyEnrichmentWorker stopped");
    }

    private async Task LoadBlacklistedDomainsAsync(CancellationToken ct)
    {
        var domains = await apiClient.GetDomainsToSkipAsync(ct);
        serperService.SetBlacklistedDomains(domains);
        logger.LogInformation("Loaded {Count} blacklisted domains from API", domains.Count);
    }

    private async Task<EnrichmentResult> EnrichCompanyAsync(Company company, CancellationToken ct)
    {
        var query = serperService.BuildSearchQuery(company);
        logger.LogInformation("Searching Serper for: {Query}", query);

        var results = await serperService.SearchAsync(query, ct);
        if (results.Count == 0)
        {
            logger.LogInformation("No search results for company {Name}", company.Name);
            return new EnrichmentResult("", null, "");
        }

        // Track domain frequency and auto-blacklist domains that appear too often
        var promotedDomains = serperService.RecordDomains(results);
        foreach (var domain in promotedDomains)
        {
            logger.LogInformation("Auto-blacklisted domain {Domain} (appeared 25+ times in search results)", domain);
            await apiClient.CreateDomainToSkipAsync(domain, ct);
        }

        logger.LogInformation("Got {Count} search results for {Name}:{Urls}",
            results.Count, company.Name,
            string.Concat(results.Select(r => $"\n  - {r.Link}")));

        foreach (var result in results)
        {
            if (serperService.IsPdfUrl(result.Link))
            {
                logger.LogInformation("Skipping PDF URL: {Url}", result.Link);
                continue;
            }

            if (serperService.IsBlacklistedUrl(result.Link))
            {
                logger.LogInformation("Skipping blacklisted URL: {Url}", result.Link);
                continue;
            }

            FetchResult fetchResult;
            try
            {
                fetchResult = await webScrapingService.FetchAndExtractTextAsync(result.Link, ct);
            }
            catch (HttpRequestException ex)
            {
                logger.LogInformation("Skipping {Url}: {Message}", result.Link, ex.InnerException?.Message ?? ex.Message);
                continue;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to fetch {Url}, trying next result", result.Link);
                continue;
            }

            if (string.IsNullOrWhiteSpace(fetchResult.PageText))
            {
                logger.LogDebug("Empty page text for {Url}, skipping", result.Link);
                continue;
            }

            // Try to fetch Impressum page if a link was found
            string? impressumText = null;
            if (fetchResult.ImpressumUrl is not null)
            {
                logger.LogInformation("Found Impressum link: {ImpressumUrl}", fetchResult.ImpressumUrl);
                try
                {
                    impressumText = await webScrapingService.FetchPlainTextAsync(fetchResult.ImpressumUrl, ct);
                }
                catch (HttpRequestException ex)
                {
                    logger.LogInformation("Skipping Impressum {Url}: {Message}", fetchResult.ImpressumUrl, ex.InnerException?.Message ?? ex.Message);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to fetch Impressum at {Url}", fetchResult.ImpressumUrl);
                }
            }

            // Cross-match: only when enabled and we actually have an Impressum/Imprint URL
            if (_crossMatchingEnabled && impressumText is not null)
            {
                var crossMatchUrl = fetchResult.ImpressumUrl ?? result.Link;
                if (crossMatchUrl.Contains("impressum", StringComparison.OrdinalIgnoreCase)
                    || crossMatchUrl.Contains("imprint", StringComparison.OrdinalIgnoreCase))
                {
                    await TryCrossMatchAsync(impressumText, fetchResult.PageText, result.Link, fetchResult.ImpressumUrl, company.Id, ct);
                }
            }

            // Hard check: if Impressum contains company name, that's a strong match
            var impressumMatch = false;
            if (impressumText is not null && company.Name is not null)
            {
                impressumMatch = matchingService.CheckImpressumMatch(impressumText, company.Name);
                if (impressumMatch)
                    logger.LogInformation("Impressum match found for {Name} at {Url}", company.Name, result.Link);
            }

            var confidence = impressumMatch
                ? 1.0
                : await ollamaService.GetConfidenceScoreAsync(fetchResult.PageText, impressumText, company, ct);
            logger.LogInformation("Confidence for {Url}: {Confidence:F2}{Detail}",
                result.Link, confidence,
                impressumMatch ? " (Impressum match)" : impressumText is not null ? " (with Impressum)" : "");

            if (confidence > ConfidenceThreshold)
            {
                var sectorList = sectorCacheService.GetSectorList();
                var (sectorName, activity) = await ollamaService.ExtractSectorAndActivityAsync(fetchResult.PageText, company, sectorList, ct);

                Guid? sectorId = null;
                if (!string.IsNullOrEmpty(sectorName))
                    sectorId = await sectorCacheService.GetOrCreateSectorAsync(sectorName, ct);

                var website = webScrapingService.NormalizeToBaseUrl(result.Link);

                logger.LogInformation("Match found for {Name}: {Website}", company.Name, website);
                return new EnrichmentResult(website, sectorId, activity, fetchResult.ImpressumUrl);
            }
        }

        logger.LogInformation("No confident match found for {Name}", company.Name);
        return new EnrichmentResult("", null, "");
    }

    /// <summary>
    /// Cross-match: check if the Impressum text matches any OTHER company in the cache.
    /// If found, create a Website entity and enrich the company with Sector + Activity.
    /// </summary>
    private async Task TryCrossMatchAsync(
        string impressumText, string pageText, string resultUrl,
        string? impressumUrl, Guid currentCompanyId, CancellationToken ct)
    {
        try
        {
            var match = matchingService.FindMatchInImpressum(impressumText);
            if (match is null || match.CompanyId == currentCompanyId)
                return;

            logger.LogInformation(
                "Cross-match found! Impressum at {Url} matches company {Id} ({Name})",
                resultUrl, match.CompanyId, match.CompanyName);

            // Extract Sector and Activity for the matched company
            var tempCompany = new Company { Name = match.CompanyName };
            var sectorList = sectorCacheService.GetSectorList();
            var (sectorName, activity) = await ollamaService.ExtractSectorAndActivityAsync(pageText, tempCompany, sectorList, ct);

            Guid? sectorId = null;
            if (!string.IsNullOrEmpty(sectorName))
                sectorId = await sectorCacheService.GetOrCreateSectorAsync(sectorName, ct);

            var website = webScrapingService.NormalizeToBaseUrl(resultUrl);

            // Create Website entity for the cross-matched company
            await apiClient.CreateWebsiteAsync(match.CompanyId, website, impressumUrl, ct);

            // PATCH the cross-matched company with sector, activity, and mark as checked
            await apiClient.PatchCompanyAsync(match.CompanyId, sectorId, activity, DateTime.UtcNow, ct);
            logger.LogInformation(
                "Cross-matched company {Id} ({Name}) enriched: website={Website}, sectorId={SectorId}, activity={Activity}",
                match.CompanyId, match.CompanyName, website, sectorId, activity);

            // Remove from cache so it won't be matched again
            matchingService.RemoveFromCache(match.CompanyId);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Cross-match processing failed for Impressum at {Url}", resultUrl);
        }
    }
}
