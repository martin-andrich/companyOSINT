using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace companyOSINT.Infrastructure.Turnstile;

public interface ITurnstileValidationService
{
    Task<bool> ValidateAsync(string token);
}

public class TurnstileValidationService(HttpClient httpClient, IOptions<TurnstileSettings> options)
    : ITurnstileValidationService
{
    private readonly string _secretKey = options.Value.SecretKey;

    public async Task<bool> ValidateAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["secret"] = _secretKey,
            ["response"] = token
        });

        var response = await httpClient.PostAsync("https://challenges.cloudflare.com/turnstile/siteverify", content);

        if (!response.IsSuccessStatusCode)
            return false;

        var result = await response.Content.ReadFromJsonAsync<TurnstileResponse>();
        return result?.Success == true;
    }

    private sealed class TurnstileResponse
    {
        public bool Success { get; set; }
    }
}
