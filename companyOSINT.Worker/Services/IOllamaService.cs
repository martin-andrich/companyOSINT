using companyOSINT.Domain.Entities;

namespace companyOSINT.Worker.Services;

public interface IOllamaService
{
    Task<double> GetConfidenceScoreAsync(string pageText, string? impressumText, Company company, CancellationToken ct);
    Task<(string SectorName, string Activity)> ExtractSectorAndActivityAsync(string pageText, Company company, string availableSectors, CancellationToken ct);
}
