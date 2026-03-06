namespace companyOSINT.Worker.Models;

public record EnrichmentResult(string UrlWebsite, Guid? SectorId, string Activity, string? UrlImprint = null);
