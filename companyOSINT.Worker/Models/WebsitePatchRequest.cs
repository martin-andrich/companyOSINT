namespace companyOSINT.Worker.Models;

public record WebsitePatchRequest(
    int HttpResponseCode,
    string? IpAddress,
    bool SslValid,
    double AverageTimeToFirstByte,
    DateTime DateLastChecked,
    bool ConsentManagerFound,
    int RequestsWithoutConsent,
    int CookiesWithoutConsent);
