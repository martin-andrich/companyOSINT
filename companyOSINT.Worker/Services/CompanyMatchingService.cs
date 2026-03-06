using System.Text.RegularExpressions;
using companyOSINT.Domain.Common;
using companyOSINT.Worker.Matching;
using Microsoft.Extensions.Logging;

namespace companyOSINT.Worker.Services;

public partial class CompanyMatchingService(
    ICompanyApiClient apiClient,
    ILogger<CompanyMatchingService> logger) : ICompanyMatchingService
{
    private const int MinCoreNameLength = 4;
    private const double TrigramThreshold = 0.6;

    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    // Tier 1: Normalized name → list of companies
    private Dictionary<string, List<CompanyNameDto>> _normalizedIndex = new();

    // Tier 2: Cologne phonetic code → list of companies
    private Dictionary<string, List<CompanyNameDto>> _phoneticIndex = new();

    // Tier 3: Precomputed trigrams for each company
    private List<(CompanyNameDto Company, string NormalizedName, HashSet<string> Trigrams)> _trigramEntries = [];

    public async Task RefreshCacheAsync(CancellationToken ct)
    {
        await _refreshLock.WaitAsync(ct);
        try
        {
            var normalizedIndex = new Dictionary<string, List<CompanyNameDto>>();
            var phoneticIndex = new Dictionary<string, List<CompanyNameDto>>();
            var trigramEntries = new List<(CompanyNameDto, string, HashSet<string>)>();

            var totalCount = 0;
            await foreach (var batch in apiClient.GetNamesToCheckBatchedAsync(ct))
            {
                totalCount += batch.Count;
                logger.LogInformation("Matching cache: loaded batch of {BatchCount} (total so far: {Total})",
                    batch.Count, totalCount);

                foreach (var company in batch)
                {
                    if (string.IsNullOrWhiteSpace(company.Name))
                        continue;

                    if (!NameNormalizer.HasLegalForm(company.Name))
                        continue;

                    var coreName = NameNormalizer.RemoveLegalForm(company.Name);
                    if (coreName.Length < MinCoreNameLength)
                        continue;

                    // Normalized index
                    var normalized = NameNormalizer.Normalize(company.Name);
                    if (!normalizedIndex.TryGetValue(normalized, out var normalizedList))
                    {
                        normalizedList = [];
                        normalizedIndex[normalized] = normalizedList;
                    }
                    normalizedList.Add(company);

                    // Phonetic index
                    var phoneticCode = ColognePhonetics.Encode(normalized);
                    if (phoneticCode.Length > 0)
                    {
                        if (!phoneticIndex.TryGetValue(phoneticCode, out var phoneticList))
                        {
                            phoneticList = [];
                            phoneticIndex[phoneticCode] = phoneticList;
                        }
                        phoneticList.Add(company);
                    }

                    // Trigram index
                    var trigrams = TrigramSimilarity.GetTrigrams(normalized);
                    trigramEntries.Add((company, normalized, trigrams));
                }
            }

            var indexedCount = normalizedIndex.Values.Sum(l => l.Count);
            logger.LogInformation(
                "Matching cache complete: {Indexed} of {Total} companies indexed (legal form filter)",
                indexedCount, totalCount);

            // Atomic swap
            _normalizedIndex = normalizedIndex;
            _phoneticIndex = phoneticIndex;
            _trigramEntries = trigramEntries;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    public CompanyMatchResult? FindMatchInImpressum(string impressumText)
    {
        // Extract candidate company name fragments from Impressum text
        // Look for segments that contain legal form suffixes — these are likely company names
        var candidates = ExtractCompanyNameCandidates(impressumText);
        if (candidates.Count == 0)
            return null;

        foreach (var candidate in candidates)
        {
            var normalized = NameNormalizer.Normalize(candidate);
            if (normalized.Length < MinCoreNameLength)
                continue;

            // Tier 1: Exact normalized match
            var normalizedIndex = _normalizedIndex; // snapshot
            if (normalizedIndex.TryGetValue(normalized, out var exactMatches))
            {
                foreach (var match in exactMatches)
                {
                    if (VerifyAddress(impressumText, match.RegisteredOffice))
                        return new CompanyMatchResult(match.Id, match.Name!);
                }
            }

            // Tier 2: Phonetic match
            var phoneticCode = ColognePhonetics.Encode(normalized);
            var phoneticIndex = _phoneticIndex; // snapshot
            if (phoneticCode.Length > 0 && phoneticIndex.TryGetValue(phoneticCode, out var phoneticMatches))
            {
                foreach (var match in phoneticMatches)
                {
                    if (VerifyAddress(impressumText, match.RegisteredOffice))
                        return new CompanyMatchResult(match.Id, match.Name!);
                }
            }

            // Tier 3: Trigram similarity
            var candidateTrigrams = TrigramSimilarity.GetTrigrams(normalized);
            var trigramEntries = _trigramEntries; // snapshot
            var bestScore = 0.0;
            CompanyNameDto? bestMatch = null;

            foreach (var (company, _, trigrams) in trigramEntries)
            {
                var similarity = TrigramSimilarity.JaccardSimilarity(candidateTrigrams, trigrams);
                if (similarity > bestScore && VerifyAddress(impressumText, company.RegisteredOffice))
                {
                    bestScore = similarity;
                    bestMatch = company;
                }
            }

            if (bestScore > TrigramThreshold && bestMatch?.Name is not null)
                return new CompanyMatchResult(bestMatch.Id, bestMatch.Name);
        }

        return null;
    }

    public bool CheckImpressumMatch(string impressumText, string companyName)
    {
        var candidates = ExtractCompanyNameCandidates(impressumText);
        var targetNormalized = NameNormalizer.Normalize(companyName);
        if (targetNormalized.Length < MinCoreNameLength)
            return false;

        foreach (var candidate in candidates)
        {
            var normalized = NameNormalizer.Normalize(candidate);
            if (normalized == targetNormalized)
                return true;
        }
        return false;
    }

    public void RemoveFromCache(Guid companyId)
    {
        _trigramEntries = _trigramEntries.Where(e => e.Company.Id != companyId).ToList();

        // Remove from dictionary indices
        foreach (var list in _normalizedIndex.Values)
            list.RemoveAll(c => c.Id == companyId);

        foreach (var list in _phoneticIndex.Values)
            list.RemoveAll(c => c.Id == companyId);
    }

    private static bool VerifyAddress(string impressumText, string? registeredOffice)
    {
        if (string.IsNullOrWhiteSpace(registeredOffice))
            return true;
        return ContainsAsWholeWord(impressumText, registeredOffice);
    }

    private static bool ContainsAsWholeWord(string text, string word)
    {
        var index = 0;
        while ((index = text.IndexOf(word, index, StringComparison.OrdinalIgnoreCase)) >= 0)
        {
            var before = index > 0 ? text[index - 1] : ' ';
            var after = index + word.Length < text.Length ? text[index + word.Length] : ' ';
            if (!char.IsLetterOrDigit(before) && !char.IsLetterOrDigit(after))
                return true;
            index += 1;
        }
        return false;
    }

    /// <summary>
    /// Extract segments from Impressum text that look like company names
    /// (i.e., they contain a legal form suffix like GmbH, UG, AG, etc.)
    /// </summary>
    private static List<string> ExtractCompanyNameCandidates(string impressumText)
    {
        var candidates = new List<string>();
        var matches = LegalFormPatternRegex().Matches(impressumText);

        foreach (Match match in matches)
        {
            // Take up to 80 chars before the legal form as the potential company name
            var legalFormStart = match.Index;
            var lineStart = impressumText.LastIndexOfAny(['\n', '\r', '.', ',', ':'], Math.Max(0, legalFormStart - 1));
            lineStart = lineStart < 0 ? 0 : lineStart + 1;

            var candidate = impressumText[lineStart..(legalFormStart + match.Length)].Trim();
            if (candidate.Length is >= 4 and <= 120)
                candidates.Add(candidate);
        }

        return candidates;
    }

    [GeneratedRegex(
        @"\b(?:GmbH\s*&\s*Co\.\s*KG|GmbH|mbH|UG\s*\(haftungsbeschr(?:a|ae|ä)nkt\)|UG\b|AG\b|e\.V\.|e\.G\.|KG\b|OHG\b|SE\b|Ltd\.|Inc\.)",
        RegexOptions.IgnoreCase)]
    private static partial Regex LegalFormPatternRegex();
}
