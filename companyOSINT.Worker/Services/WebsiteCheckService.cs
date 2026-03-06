using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using companyOSINT.Worker.Models;
using Microsoft.Extensions.Logging;

namespace companyOSINT.Worker.Services;

public class WebsiteCheckService(ILogger<WebsiteCheckService> logger) : IWebsiteCheckService
{
    private const int MaxBodySize = 524_288; // 512 KB

    public async Task<WebsiteCheckResult> CheckAsync(string url, CancellationToken ct)
    {
        var uri = new Uri(url);
        var host = uri.Host;

        var ipAddress = await ResolveDnsAsync(host, ct);
        var sslValid = await CheckSslAsync(host, ct);
        var (httpResponseCode, averageTtfb, htmlBody, responseHeaders) = await MeasureHttpAsync(url, ct);

        return new WebsiteCheckResult(httpResponseCode, ipAddress, sslValid, averageTtfb, htmlBody, responseHeaders);
    }

    private async Task<string?> ResolveDnsAsync(string host, CancellationToken ct)
    {
        try
        {
            var addresses = await Dns.GetHostAddressesAsync(host, ct);
            var preferred = addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork)
                            ?? addresses.FirstOrDefault();
            return preferred?.ToString();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "DNS resolution failed for {Host}", host);
            return null;
        }
    }

    private async Task<bool> CheckSslAsync(string host, CancellationToken ct)
    {
        try
        {
            using var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(host, 443, ct);
            using var sslStream = new SslStream(tcpClient.GetStream(), false,
                (_, _, _, errors) => errors == SslPolicyErrors.None);
            await sslStream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
            {
                TargetHost = host
            }, ct);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogInformation("SSL check failed for {Host}: {Message}", host, ex.Message);
            return false;
        }
    }

    private async Task<(int StatusCode, double AverageTtfbMs, string? HtmlBody,
        Dictionary<string, IEnumerable<string>> ResponseHeaders)> MeasureHttpAsync(string url, CancellationToken ct)
    {
        using var handler = new HttpClientHandler
        {
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 5,
            AutomaticDecompression = DecompressionMethods.All,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };
        using var client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36");

        int statusCode = 0;
        double ttfbMs = 0;
        string? htmlBody = null;
        var responseHeaders = new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase);

        try
        {
            var sw = Stopwatch.StartNew();
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
            sw.Stop();

            statusCode = (int)response.StatusCode;
            ttfbMs = sw.Elapsed.TotalMilliseconds;

            // Capture response headers
            foreach (var header in response.Headers)
                responseHeaders[header.Key] = header.Value.ToList();
            foreach (var header in response.Content.Headers)
                responseHeaders[header.Key] = header.Value.ToList();

            // Read body if HTML and successful
            if (response.IsSuccessStatusCode)
            {
                var contentType = response.Content.Headers.ContentType?.MediaType ?? "";
                if (contentType.Contains("html", StringComparison.OrdinalIgnoreCase))
                {
                    var bodyBytes = await response.Content.ReadAsByteArrayAsync(ct);
                    if (bodyBytes.Length <= MaxBodySize)
                        htmlBody = Encoding.UTF8.GetString(bodyBytes);
                }
            }
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            logger.LogWarning("HTTP request timed out for {Url}", url);
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "HTTP request failed for {Url}", url);
        }

        return (statusCode, ttfbMs, htmlBody, responseHeaders);
    }
}
