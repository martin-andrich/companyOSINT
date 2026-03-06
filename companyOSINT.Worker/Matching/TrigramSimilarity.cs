namespace companyOSINT.Worker.Matching;

public static class TrigramSimilarity
{
    /// <summary>
    /// Generate the set of trigrams (3-character sliding window) for a string.
    /// </summary>
    public static HashSet<string> GetTrigrams(string input)
    {
        if (input.Length < 3)
            return [input];

        var trigrams = new HashSet<string>(input.Length - 2);
        for (var i = 0; i <= input.Length - 3; i++)
            trigrams.Add(input.Substring(i, 3));
        return trigrams;
    }

    /// <summary>
    /// Jaccard similarity: |A ∩ B| / |A ∪ B|
    /// </summary>
    public static double JaccardSimilarity(HashSet<string> a, HashSet<string> b)
    {
        if (a.Count == 0 && b.Count == 0) return 1.0;
        if (a.Count == 0 || b.Count == 0) return 0.0;

        var intersection = a.Count < b.Count
            ? a.Count(x => b.Contains(x))
            : b.Count(x => a.Contains(x));

        var union = a.Count + b.Count - intersection;
        return union == 0 ? 0.0 : (double)intersection / union;
    }
}
