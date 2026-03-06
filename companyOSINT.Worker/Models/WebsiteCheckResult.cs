namespace companyOSINT.Worker.Models;

public record WebsiteCheckResult(
    int HttpResponseCode,
    string? IpAddress,
    bool SslValid,
    double AverageTimeToFirstByte,
    string? HtmlBody,
    IReadOnlyDictionary<string, IEnumerable<string>> ResponseHeaders);
