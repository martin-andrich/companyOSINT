using companyOSINT.Domain.Dtos.ApiTokens;
using companyOSINT.Domain.Entities;

namespace companyOSINT.Application.Interfaces;

public interface IApiTokenService
{
    Task<List<ApiToken>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    Task<(ApiToken Token, string PlainTextToken)> CreateAsync(Guid userId, ApiTokenCreateDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid tokenId, Guid userId, CancellationToken ct = default);
    Task<ApiToken?> ValidateTokenAsync(string plainTextToken, CancellationToken ct = default);
}
