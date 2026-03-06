namespace companyOSINT.Worker.Matching;

/// <summary>
/// Implementation of the Kölner Phonetik (Cologne Phonetics) algorithm
/// for phonetic encoding of German words.
/// </summary>
public static class ColognePhonetics
{
    public static string Encode(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "";

        // Normalize: uppercase, replace umlauts
        var normalized = input.ToUpperInvariant()
            .Replace("Ä", "AE")
            .Replace("Ö", "OE")
            .Replace("Ü", "UE")
            .Replace("ß", "S");

        // Remove non-letter characters
        var letters = new char[normalized.Length];
        var len = 0;
        foreach (var c in normalized)
        {
            if (c is >= 'A' and <= 'Z')
                letters[len++] = c;
        }

        if (len == 0)
            return "";

        var codes = new char[len * 2]; // max possible (X can produce 2 digits)
        var codeLen = 0;

        for (var i = 0; i < len; i++)
        {
            var c = letters[i];
            var prev = i > 0 ? letters[i - 1] : '\0';
            var next = i < len - 1 ? letters[i + 1] : '\0';

            var code = GetCode(c, prev, next, i == 0);
            if (code != null)
            {
                foreach (var digit in code)
                    codes[codeLen++] = digit;
            }
        }

        if (codeLen == 0)
            return "0";

        // Remove consecutive duplicates
        var result = new char[codeLen];
        result[0] = codes[0];
        var resultLen = 1;
        for (var i = 1; i < codeLen; i++)
        {
            if (codes[i] != codes[i - 1])
                result[resultLen++] = codes[i];
        }

        // Remove all '0' except if it's the first character
        var final_ = new char[resultLen];
        final_[0] = result[0];
        var finalLen = 1;
        for (var i = 1; i < resultLen; i++)
        {
            if (result[i] != '0')
                final_[finalLen++] = result[i];
        }

        return new string(final_, 0, finalLen);
    }

    private static string? GetCode(char c, char prev, char next, bool isFirst)
    {
        switch (c)
        {
            case 'A' or 'E' or 'I' or 'O' or 'U' or 'J' or 'Y':
                return "0";

            case 'H':
                return null; // ignored

            case 'B':
                return "1";

            case 'P':
                return next == 'H' ? "3" : "1";

            case 'D' or 'T':
                return next is 'C' or 'S' or 'Z' ? "8" : "2";

            case 'F' or 'V' or 'W':
                return "3";

            case 'G' or 'K' or 'Q':
                return "4";

            case 'C':
                if (isFirst)
                    return next is 'A' or 'H' or 'K' or 'L' or 'O' or 'Q' or 'R' or 'U' or 'X' ? "4" : "8";
                return prev is 'S' or 'Z' ? "8"
                    : next is 'A' or 'H' or 'K' or 'O' or 'Q' or 'U' or 'X' ? "4"
                    : "8";

            case 'X':
                return prev is 'C' or 'K' or 'Q' ? "8" : "48";

            case 'L':
                return "5";

            case 'M' or 'N':
                return "6";

            case 'R':
                return "7";

            case 'S' or 'Z':
                return "8";

            default:
                return null;
        }
    }
}
