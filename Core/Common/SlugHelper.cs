namespace Core.Common;

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

public static class SlugHelper
{
    private static readonly Regex _invalidChars = new(@"[^a-z0-9\-]+", RegexOptions.Compiled);
    private static readonly Regex _multiDash = new(@"\-+", RegexOptions.Compiled);

    public static string ToSlug(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "item";

        input = input.Trim().ToLowerInvariant();

        var norm = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var ch in norm)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }
        var result = sb.ToString().Normalize(NormalizationForm.FormC);

        result = result.Replace(' ', '-');
        result = _invalidChars.Replace(result, "");
        result = _multiDash.Replace(result, "-").Trim('-');

        return string.IsNullOrEmpty(result) ? "item" : result;
    }
}
