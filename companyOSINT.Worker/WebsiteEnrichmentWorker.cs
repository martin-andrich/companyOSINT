using companyOSINT.Worker.Detection;
using companyOSINT.Worker.Models;
using companyOSINT.Worker.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace companyOSINT.Worker;

public class WebsiteEnrichmentWorker(
    ICompanyApiClient apiClient,
    IWebsiteCheckService checkService,
    IDetectionEngine detectionEngine,
    IConsentCheckService consentCheckService,
    ILogger<WebsiteEnrichmentWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("WebsiteEnrichmentWorker started");

        await consentCheckService.InitializeAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var website = await apiClient.GetNextWebsiteToEnrichAsync(stoppingToken);

                if (website is null)
                {
                    logger.LogInformation("No websites to enrich, waiting 30s...");
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                    continue;
                }

                logger.LogInformation("Checking website {Id} ({Url})", website.Id, website.UrlWebsite);

                WebsiteCheckResult result;
                try
                {
                    result = await checkService.CheckAsync(website.UrlWebsite!, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Check failed for website {Id} ({Url}), marking as checked with failure data",
                        website.Id, website.UrlWebsite);
                    result = new WebsiteCheckResult(0, null, false, 0, null,
                        new Dictionary<string, IEnumerable<string>>());
                }

                // Software + Tool detection
                DetectionResult detectionResult = new([], []);
                if (result.HtmlBody is not null)
                {
                    detectionResult = detectionEngine.DetectAll(result.HtmlBody, result.ResponseHeaders, website.UrlWebsite!);
                    if (detectionResult.Software.Count > 0)
                    {
                        logger.LogInformation("Website {Id}: detected {Count} software: {Names}",
                            website.Id, detectionResult.Software.Count,
                            string.Join(", ", detectionResult.Software.Select(s => s.Name)));
                    }
                    if (detectionResult.Tools.Count > 0)
                    {
                        logger.LogInformation("Website {Id}: detected {Count} tools: {Names}",
                            website.Id, detectionResult.Tools.Count,
                            string.Join(", ", detectionResult.Tools.Select(t => t.Name)));
                    }
                }

                // Consent check (only for websites with successful HTTP response)
                var consentResult = new ConsentCheckResult(false, 0, 0);
                if (result.HttpResponseCode is >= 200 and < 300)
                {
                    try
                    {
                        consentResult = await consentCheckService.CheckAsync(website.UrlWebsite!, stoppingToken);
                        logger.LogInformation(
                            "Website {Id}: consentManager={CmpFound}, thirdPartyRequests={Requests}, cookies={Cookies}",
                            website.Id, consentResult.ConsentManagerFound,
                            consentResult.RequestsWithoutConsent, consentResult.CookiesWithoutConsent);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Consent check failed for website {Id}", website.Id);
                    }
                }

                logger.LogInformation(
                    "Website {Id}: status={StatusCode}, ip={IpAddress}, ssl={SslValid}, ttfb={Ttfb:F0}ms",
                    website.Id, result.HttpResponseCode, result.IpAddress ?? "(unresolved)",
                    result.SslValid, result.AverageTimeToFirstByte);

                await apiClient.PatchWebsiteAsync(
                    website.Id,
                    new WebsitePatchRequest(
                        result.HttpResponseCode,
                        result.IpAddress,
                        result.SslValid,
                        result.AverageTimeToFirstByte,
                        DateTime.UtcNow,
                        consentResult.ConsentManagerFound,
                        consentResult.RequestsWithoutConsent,
                        consentResult.CookiesWithoutConsent),
                    stoppingToken);

                if (detectionResult.Software.Count > 0)
                {
                    await apiClient.ReplaceSoftwareAsync(website.Id, detectionResult.Software, stoppingToken);
                }

                if (detectionResult.Tools.Count > 0)
                {
                    await apiClient.ReplaceToolsAsync(website.Id, detectionResult.Tools, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing website, retrying in 10s...");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        logger.LogInformation("WebsiteEnrichmentWorker stopped");
    }
}
