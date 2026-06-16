using System.Globalization;
using System.Text;

namespace Phipes.Blog.Services;

/// <summary>Genera slugs URL-safe a partir de un título (quita tildes, espacios → guiones).</summary>
public static class Slug
{
    public static string From(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        // Descompone y elimina marcas diacríticas (á → a).
        var normalized = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark) continue;
            if (char.IsLetterOrDigit(ch)) sb.Append(char.ToLowerInvariant(ch));
            else if (ch is ' ' or '-' or '_' or '.' or '/') sb.Append('-');
        }

        var slug = sb.ToString();
        while (slug.Contains("--")) slug = slug.Replace("--", "-");
        return slug.Trim('-');
    }
}
