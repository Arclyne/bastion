using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Bastion.Domain.Chat;

public static class ChatWordFilter
{
    private const char WordSeparator = ' ';

    public static bool ContainsBannedTerm(string text, IEnumerable<string> bannedTerms)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(bannedTerms);

        string comparableText = ToComparableWords(text);
        return bannedTerms
            .Select(ToComparableWords)
            .Where(term => !string.IsNullOrWhiteSpace(term))
            .Any(term => comparableText.Contains(term, StringComparison.Ordinal));
    }

    private static string ToComparableWords(string text)
    {
        string decomposed = text.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        builder.Append(WordSeparator);
        foreach (char character in decomposed)
        {
            AppendComparable(builder, character);
        }

        if (builder[^1] != WordSeparator)
        {
            builder.Append(WordSeparator);
        }

        return builder.ToString();
    }

    private static void AppendComparable(StringBuilder builder, char character)
    {
        if (char.IsLetterOrDigit(character))
        {
            builder.Append(char.ToLowerInvariant(character));
        }
        else if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark
            && builder[^1] != WordSeparator)
        {
            builder.Append(WordSeparator);
        }
    }
}
