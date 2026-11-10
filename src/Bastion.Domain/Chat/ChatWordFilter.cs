using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Bastion.Domain.Chat;

public static class ChatWordFilter
{
    private const char WordSeparator = ' ';

    public static bool HasBannedTerm(string text, IEnumerable<string> bannedTerms)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(bannedTerms);

        string normalizedText = NormalizeWords(text);
        bool hasBannedTerm = bannedTerms
            .Select(NormalizeWords)
            .Where(term => !string.IsNullOrWhiteSpace(term))
            .Any(term => normalizedText.Contains(term, StringComparison.Ordinal));
        return hasBannedTerm;
    }

    private static string NormalizeWords(string text)
    {
        string decomposedText = text.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposedText.Length);
        builder.Append(WordSeparator);
        foreach (char character in decomposedText)
        {
            AppendNormalizedCharacter(builder, character);
        }

        AppendSeparator(builder);
        return builder.ToString();
    }

    private static void AppendNormalizedCharacter(StringBuilder builder, char character)
    {
        if (char.IsLetterOrDigit(character))
        {
            builder.Append(char.ToLowerInvariant(character));
        }
        else if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
        {
            AppendSeparator(builder);
        }
    }

    private static void AppendSeparator(StringBuilder builder)
    {
        if (builder[^1] != WordSeparator)
        {
            builder.Append(WordSeparator);
        }
    }
}
