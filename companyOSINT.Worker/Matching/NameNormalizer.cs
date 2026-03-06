using System.Text.RegularExpressions;

namespace companyOSINT.Worker.Matching;

public static partial class NameNormalizer
{
    /// <summary>
    /// Normalize a company name for exact matching:
    /// Remove legal form → lowercase → normalize umlauts → remove punctuation → collapse whitespace
    /// </summary>
    public static string Normalize(string name)
    {
        var result = LegalFormSuffixRegex().Replace(name, "");
        result = result.ToLowerInvariant();
        result = NormalizeUmlauts(result);
        result = PunctuationRegex().Replace(result, " ");
        result = WhitespaceRegex().Replace(result, " ");
        return result.Trim();
    }

    /// <summary>
    /// Remove legal form suffix only (for substring matching in Impressum text).
    /// </summary>
    public static string RemoveLegalForm(string name)
    {
        return LegalFormSuffixRegex().Replace(name, "").Trim();
    }

    /// <summary>
    /// Check whether the name contains a recognized legal form suffix.
    /// </summary>
    public static bool HasLegalForm(string name)
    {
        return LegalFormSuffixRegex().IsMatch(name);
    }

    private static string NormalizeUmlauts(string text)
    {
        return text
            .Replace("ä", "ae")
            .Replace("ö", "oe")
            .Replace("ü", "ue")
            .Replace("ß", "ss");
    }

    [GeneratedRegex(
        @"\s*(GmbH\s*&\s*Co\.\s*KG|GmbH|mbH|UG\s*\(haftungsbeschr(?:a|ae|ä)nkt\)|UG|AG|e\.V\.|e\.G\.|KG|OHG|SE|Ltd\.|Inc\.)\s*$",
        RegexOptions.IgnoreCase)]
    public static partial Regex LegalFormSuffixRegex();

    [GeneratedRegex(@"[^\w\s]")]
    private static partial Regex PunctuationRegex();

    [GeneratedRegex(@"\s+")]
    public static partial Regex WhitespaceRegex();
}
