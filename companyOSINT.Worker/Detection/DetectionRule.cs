namespace companyOSINT.Worker.Detection;

public enum RuleType
{
    MetaGenerator,
    HtmlContains,
    HeaderExists,
    HeaderContains
}

public record DetectionRule(
    RuleType Type,
    string Pattern,
    string? HeaderName = null);
