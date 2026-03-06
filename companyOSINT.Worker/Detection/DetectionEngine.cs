using System.Text.RegularExpressions;
using companyOSINT.Worker.Models;
using Microsoft.Extensions.Logging;

namespace companyOSINT.Worker.Detection;

public interface IDetectionEngine
{
    DetectionResult DetectAll(string htmlContent, IReadOnlyDictionary<string, IEnumerable<string>> responseHeaders, string url);
}

public partial class DetectionEngine(ILogger<DetectionEngine> logger) : IDetectionEngine
{
    public DetectionResult DetectAll(string htmlContent, IReadOnlyDictionary<string, IEnumerable<string>> responseHeaders, string url)
    {
        var software = new List<SoftwareDetection>();
        var tools = new List<ToolDetection>();
        var detectedNames = new HashSet<string>();

        // First pass: detectors without parent requirements
        foreach (var descriptor in Descriptors.All)
        {
            if (descriptor.RequiresParent is not null)
                continue;

            TryDetect(descriptor, htmlContent, responseHeaders, url, software, tools, detectedNames);
        }

        // Second pass: detectors with parent requirements (only if parent was detected)
        foreach (var descriptor in Descriptors.All)
        {
            if (descriptor.RequiresParent is null)
                continue;
            if (!detectedNames.Contains(descriptor.RequiresParent.Name))
                continue;

            TryDetect(descriptor, htmlContent, responseHeaders, url, software, tools, detectedNames);
        }

        if (software.Count > 0)
            logger.LogDebug("Detected {Count} software: {Names}",
                software.Count, string.Join(", ", software.Select(s => s.Name)));

        if (tools.Count > 0)
            logger.LogDebug("Detected {Count} tools: {Names}",
                tools.Count, string.Join(", ", tools.Select(t => t.Name)));

        return new DetectionResult(software, tools);
    }

    private static void TryDetect(
        DetectorDescriptor descriptor,
        string htmlContent,
        IReadOnlyDictionary<string, IEnumerable<string>> headers,
        string url,
        List<SoftwareDetection> software,
        List<ToolDetection> tools,
        HashSet<string> detectedNames)
    {
        foreach (var rule in descriptor.Rules)
        {
            var version = EvaluateRule(rule, htmlContent, headers);
            if (version is null)
                continue;

            detectedNames.Add(descriptor.Name);

            if (descriptor.Kind == DetectorKind.Software)
            {
                software.Add(new SoftwareDetection(
                    descriptor.Name,
                    version,
                    url));
            }
            else
            {
                tools.Add(new ToolDetection(
                    descriptor.Name,
                    url));
            }

            return; // First match wins for this descriptor
        }
    }

    private static string? EvaluateRule(
        DetectionRule rule,
        string html,
        IReadOnlyDictionary<string, IEnumerable<string>> headers)
    {
        switch (rule.Type)
        {
            case RuleType.MetaGenerator:
                var (found, version) = TryMetaGenerator(html, rule.Pattern);
                if (found)
                    return version;
                return null;

            case RuleType.HtmlContains:
                if (html.Contains(rule.Pattern, StringComparison.OrdinalIgnoreCase))
                    return "";
                return null;

            case RuleType.HeaderExists:
                if (headers.ContainsKey(rule.HeaderName!))
                    return "";
                return null;

            case RuleType.HeaderContains:
                if (headers.TryGetValue(rule.HeaderName!, out var values) &&
                    values.Any(v => v.Contains(rule.Pattern, StringComparison.OrdinalIgnoreCase)))
                    return "";
                return null;

            default:
                return null;
        }
    }

    private static (bool Found, string Version) TryMetaGenerator(string html, string generatorPrefix)
    {
        var match = MetaGeneratorRegex().Match(html);
        while (match.Success)
        {
            var content = match.Groups[1].Value;
            if (content.StartsWith(generatorPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var version = content.Length > generatorPrefix.Length
                    ? content[generatorPrefix.Length..].Trim()
                    : "";
                return (true, version);
            }
            match = match.NextMatch();
        }
        return (false, "");
    }

    [GeneratedRegex("""<meta\s[^>]*?name\s*=\s*["']generator["'][^>]*?content\s*=\s*["']([^"']*)["']""",
        RegexOptions.IgnoreCase)]
    private static partial Regex MetaGeneratorRegex();
}
