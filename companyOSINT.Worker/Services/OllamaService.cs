using System.Text.Json;
using System.Text.RegularExpressions;
using companyOSINT.Domain.Entities;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;

namespace companyOSINT.Worker.Services;

public partial class OllamaService(
    ChatClient chatClient,
    ILogger<OllamaService> logger) : IOllamaService
{
    public async Task<double> GetConfidenceScoreAsync(
        string pageText, string? impressumText, Company company, CancellationToken ct)
    {
        var addressParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(company.RegisteredAddress))
            addressParts.Add(company.RegisteredAddress);
        if (!string.IsNullOrWhiteSpace(company.RegisteredOffice))
            addressParts.Add(company.RegisteredOffice);

        var address = addressParts.Count > 0 ? string.Join(", ", addressParts) : "unbekannt";

        var impressumSection = impressumText is not null
            ? $"\n\nImpressum der Webseite:\n{impressumText}"
            : "";

        var userPrompt = $$"""
            Firma: {{company.Name}}, Anschrift: {{address}}
            Webseiten-Inhalt: {{pageText}}{{impressumSection}}

            Bewerte, ob diese Webseite die EIGENE offizielle Homepage der genannten Firma ist.

            HOHE Confidence (0.8–1.0): Die Seite wird von der Firma selbst betrieben (eigene Domain, eigene Inhalte, Impressum mit passendem Firmennamen/Adresse).
            MITTLERE Confidence (0.4–0.6): Unklar, ob es die eigene Seite ist, oder nur teilweise passend.
            NIEDRIGE Confidence (0.0–0.3): Die Seite gehört NICHT der Firma. Typische Fälle:
            - Firmenverzeichnisse, Branchenbücher, Auskunfteien (z.B. NorthData, Firmenwissen, Gelbe Seiten, wlw)
            - Social-Media-Profile (Facebook, LinkedIn, Xing, Instagram)
            - Nachrichtenartikel oder Blogbeiträge ÜBER die Firma
            - Seiten einer ANDEREN Firma, die die gesuchte Firma nur erwähnt

            Antworte nur mit einer Zahl zwischen 0.0 und 1.0.
            """;

        var options = new ChatCompletionOptions { MaxOutputTokenCount = 32 };

        try
        {
            var response = await chatClient.CompleteChatAsync(
            [
                new SystemChatMessage(
                    "Du bewertest, ob eine Webseite zu einer bestimmten Firma gehört. Antworte nur mit einer Zahl zwischen 0.0 und 1.0."),
                new UserChatMessage(userPrompt)
            ], options, ct);

            var text = response.Value.Content
                .Where(p => p.Kind == ChatMessageContentPartKind.Text)
                .Select(p => p.Text)
                .FirstOrDefault() ?? "";

            logger.LogDebug("Confidence response: {Response}", text);

            var match = ConfidenceRegex().Match(text);
            if (match.Success && double.TryParse(match.Value, System.Globalization.CultureInfo.InvariantCulture,
                    out var score))
                return score;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to get confidence score");
        }

        return 0.0;
    }

    public async Task<(string SectorName, string Activity)> ExtractSectorAndActivityAsync(
        string pageText, Company company, string availableSectors, CancellationToken ct)
    {
        var userPrompt = $$"""
            Analysiere den folgenden Webseiten-Inhalt und bestimme Branche und Tätigkeit der Firma "{{company.Name}}".
            Webseiten-Inhalt: {{pageText}}

            Wähle die passendste Branche aus der folgenden Liste:
            {{availableSectors}}

            Falls keine Branche aus der Liste passt, gib eine neue kurze Branchenbezeichnung auf Deutsch an.

            Antworte im JSON-Format: {"sector": "...", "activity": "..."}
            Regeln:
            - sector = EXAKT einer der Branchennamen aus der Liste oben (bevorzugt) ODER eine neue kurze Branchenbezeichnung
            - activity = kurze Tätigkeitsbeschreibung auf Deutsch (max. 1 Satz)
            """;

        var options = new ChatCompletionOptions { MaxOutputTokenCount = 256 };

        try
        {
            var response = await chatClient.CompleteChatAsync(
            [
                new SystemChatMessage("Du bist ein Recherche-Assistent. Antworte nur mit JSON."),
                new UserChatMessage(userPrompt)
            ], options, ct);

            var text = response.Value.Content
                .Where(p => p.Kind == ChatMessageContentPartKind.Text)
                .Select(p => p.Text)
                .FirstOrDefault() ?? "";

            logger.LogDebug("Sector/Activity response: {Response}", text);

            var match = JsonExtractRegex().Match(text);
            if (match.Success)
            {
                var json = JsonSerializer.Deserialize<JsonElement>(match.Value);
                var sector = json.TryGetProperty("sector", out var b) ? b.GetString() ?? "" : "";
                var activity = json.TryGetProperty("activity", out var a) ? a.GetString() ?? "" : "";
                return (sector, activity);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to extract sector/activity");
        }

        return ("", "");
    }

    [GeneratedRegex(@"[01]\.\d+|[01]")]
    private static partial Regex ConfidenceRegex();

    [GeneratedRegex(@"\{[^{}]*\}", RegexOptions.Singleline)]
    private static partial Regex JsonExtractRegex();
}
