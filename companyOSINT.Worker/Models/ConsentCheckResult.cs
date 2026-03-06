namespace companyOSINT.Worker.Models;

public record ConsentCheckResult(
    bool ConsentManagerFound,
    int RequestsWithoutConsent,
    int CookiesWithoutConsent);
