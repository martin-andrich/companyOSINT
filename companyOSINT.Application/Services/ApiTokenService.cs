using System.Security.Cryptography;
using System.Text;
using companyOSINT.Application.Interfaces;
using companyOSINT.Domain.Dtos.ApiTokens;
using companyOSINT.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace companyOSINT.Application.Services;

public class ApiTokenService(IApplicationDbContext db) : IApiTokenService
{
    public async Task<List<ApiToken>> GetByUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await db.ApiTokens
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.DateCreated)
            .ToListAsync(ct);
    }

    public async Task<(ApiToken Token, string PlainTextToken)> CreateAsync(
        Guid userId, ApiTokenCreateDto dto, CancellationToken ct = default)
    {
        var plainToken = GenerateToken();
        var hash = HashToken(plainToken);

        var token = new ApiToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = dto.Name,
            TokenHash = hash,
            TokenPrefix = plainToken[..12],
            ExpiresAt = dto.ExpiresAt,
        };

        db.ApiTokens.Add(token);
        await db.SaveChangesAsync(ct);

        return (token, plainToken);
    }

    public async Task<bool> DeleteAsync(Guid tokenId, Guid userId, CancellationToken ct = default)
    {
        var token = await db.ApiTokens
            .FirstOrDefaultAsync(t => t.Id == tokenId && t.UserId == userId, ct);

        if (token is null) return false;

        db.ApiTokens.Remove(token);
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<ApiToken?> ValidateTokenAsync(string plainTextToken, CancellationToken ct = default)
    {
        var hash = HashToken(plainTextToken);
        var token = await db.ApiTokens
            .FirstOrDefaultAsync(t => t.TokenHash == hash, ct);

        if (token is null || token.ExpiresAt <= DateTime.UtcNow)
            return null;

        token.LastUsedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return token;
    }

    private static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return $"fac_{Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=')}";
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexStringLower(bytes);
    }
}
