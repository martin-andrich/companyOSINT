using System.Collections.Concurrent;
using companyOSINT.Worker.Models;
using Microsoft.Extensions.Logging;
using Nager.PublicSuffix;
using Nager.PublicSuffix.RuleProviders;
using Nager.PublicSuffix.RuleProviders.CacheProviders;
using PuppeteerSharp;

namespace companyOSINT.Worker.Services;

public class ConsentCheckService(ILogger<ConsentCheckService> logger) : IConsentCheckService
{
    private IBrowser? _browser;
    private DomainParser? _domainParser;

    private static readonly string[] CmpScriptDomains =
    [
        "cookiebot.com",
        "cdn.cookielaw.org",
        "usercentrics.eu",
        "app.usercentrics.eu",
        "cookieyes.com",
        "borlabs.io",
        "complianz.io",
        "quantcast.mgr.consensu.org",
        "fundingchoicesmessages.google.com",
        "consentmanager.net",
        "privacy-mgmt.com"
    ];

    private static readonly string[] CmpSelectors =
    [
        "#CybotCookiebotDialog",
        "#CybotCookiebotDialogBody",
        "#onetrust-consent-sdk",
        "#onetrust-banner-sdk",
        "#usercentrics-root",
        "[data-testid=\"uc-default-wall\"]",
        "#cookie-law-info-bar",
        ".cky-consent-container",
        "#BorlabsCookieBox",
        "#cmplz-cookiebanner-container",
        "#qc-cmp2-container",
        ".fc-consent-root",
        ".gdpr-cookie-notice",
        "#sp-cc",
        "#cmpbox",
        "#cmpbox2",
        "[class*=\"cookie-consent\"]",
        "[class*=\"cookie-banner\"]",
        "[id*=\"cookie-consent\"]",
        "[id*=\"cookie-banner\"]",
        "[class*=\"consent-manager\"]",
        "[class*=\"cookiefirst\"]"
    ];

    public async Task InitializeAsync(CancellationToken ct)
    {
        // Initialize domain parser with cached public suffix list
        var cacheProvider = new LocalFileSystemCacheProvider(
            "public-suffix-list.dat", Path.GetTempPath());
        var ruleProvider = new CachedHttpRuleProvider(cacheProvider, new HttpClient());
        await ruleProvider.BuildAsync(false, ct);
        _domainParser = new DomainParser(ruleProvider);
        logger.LogInformation("Public suffix list loaded for domain parsing");

        // Download and launch Chromium
        var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync();
        _browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            Args =
            [
                "--no-sandbox",
                "--disable-setuid-sandbox",
                "--disable-dev-shm-usage",
                "--disable-gpu",
                "--disable-extensions",
                "--disable-background-networking",
                "--disable-default-apps",
                "--no-first-run"
            ]
        });
        logger.LogInformation("Chromium browser launched for consent checks");
    }

    public async Task<ConsentCheckResult> CheckAsync(string url, CancellationToken ct)
    {
        if (_browser is null || !_browser.IsConnected)
        {
            logger.LogWarning("Browser not connected, re-initializing...");
            await InitializeAsync(ct);
        }

        await using var context = await _browser!.CreateBrowserContextAsync();
        var page = await context.NewPageAsync();

        try
        {
            var pageUri = new Uri(url);
            var pageDomain = GetRegisteredDomain(pageUri.Host);

            var thirdPartyRequestCount = 0;
            var cmpScriptFound = false;

            page.Request += (_, e) =>
            {
                try
                {
                    var reqUri = new Uri(e.Request.Url);

                    // Skip non-HTTP(S) requests (data:, blob:, etc.)
                    if (reqUri.Scheme is not ("http" or "https"))
                        return;

                    var reqDomain = GetRegisteredDomain(reqUri.Host);

                    // Count third-party requests
                    if (!string.IsNullOrEmpty(reqDomain) && !string.IsNullOrEmpty(pageDomain) &&
                        !string.Equals(reqDomain, pageDomain, StringComparison.OrdinalIgnoreCase))
                    {
                        Interlocked.Increment(ref thirdPartyRequestCount);
                    }

                    // Check for CMP script domains
                    if (!cmpScriptFound && CmpScriptDomains.Any(d =>
                            reqUri.Host.Equals(d, StringComparison.OrdinalIgnoreCase) ||
                            reqUri.Host.EndsWith("." + d, StringComparison.OrdinalIgnoreCase)))
                    {
                        cmpScriptFound = true;
                    }
                }
                catch
                {
                    // Ignore malformed URLs
                }
            };

            await page.GoToAsync(url, new NavigationOptions
            {
                WaitUntil = [WaitUntilNavigation.Networkidle0],
                Timeout = 30_000
            });

            // Extra wait for lazy-loaded tracking scripts
            await Task.Delay(3000, ct);

            // Count cookies set without consent
            var cookies = await page.GetCookiesAsync();
            var cookieCount = cookies.Length;

            // CMP detection via JavaScript APIs
            var hasCmpApi = await page.EvaluateExpressionAsync<bool>(
                "typeof window.__tcfapi === 'function' || " +
                "typeof window.__gpp === 'function' || " +
                "typeof window.__cmp === 'function'");

            // CMP detection via DOM selectors
            var selectorQuery = string.Join(", ", CmpSelectors.Select(s => s.Replace("\"", "\\\"")));
            var hasCmpElement = await page.EvaluateExpressionAsync<bool>(
                $"document.querySelector(\"{selectorQuery}\") !== null");

            var consentManagerFound = hasCmpApi || hasCmpElement || cmpScriptFound;

            logger.LogDebug("Consent check for {Url}: cmp={Cmp}, thirdParty={ThirdParty}, cookies={Cookies}",
                url, consentManagerFound, thirdPartyRequestCount, cookieCount);

            return new ConsentCheckResult(consentManagerFound, thirdPartyRequestCount, cookieCount);
        }
        catch (NavigationException ex)
        {
            logger.LogWarning("Navigation failed for {Url}: {Message}", url, ex.Message);
            return new ConsentCheckResult(false, 0, 0);
        }
        catch (TimeoutException ex)
        {
            logger.LogWarning("Timeout during consent check for {Url}: {Message}", url, ex.Message);
            return new ConsentCheckResult(false, 0, 0);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Consent check failed for {Url}", url);
            return new ConsentCheckResult(false, 0, 0);
        }
        finally
        {
            try
            {
                await page.CloseAsync();
            }
            catch
            {
                // Page may already be closed
            }
        }
    }

    private string GetRegisteredDomain(string host)
    {
        if (_domainParser is null)
            return FallbackGetRegisteredDomain(host);

        try
        {
            var info = _domainParser.Parse(host);
            return info?.RegistrableDomain ?? FallbackGetRegisteredDomain(host);
        }
        catch
        {
            return FallbackGetRegisteredDomain(host);
        }
    }

    private static string FallbackGetRegisteredDomain(string host)
    {
        var parts = host.Split('.');
        return parts.Length >= 2
            ? $"{parts[^2]}.{parts[^1]}"
            : host;
    }

    public async ValueTask DisposeAsync()
    {
        if (_browser is not null)
        {
            await _browser.CloseAsync();
            _browser.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}
