using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Bastion.Domain.Text;

namespace Bastion.Domain.Friends;

public static class FriendCodePolicy
{
    public const int Length = 6;

    private const int SeparatorPosition = 2;
    private const char Separator = '-';
    private const string Alphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";

    private static readonly Regex _friendCodePattern = TextPattern.Create($"^[{Alphabet}]{{{Length}}}$");

    public static bool IsValid(string? text)
    {
        return text is not null && _friendCodePattern.IsMatch(text);
    }

    public static string Normalize(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var builder = new StringBuilder(text.Length);
        foreach (char character in text)
        {
            if (character != Separator && !char.IsWhiteSpace(character))
            {
                builder.Append(char.ToUpper(character, CultureInfo.InvariantCulture));
            }
        }

        return builder.ToString();
    }

    public static string Format(string code)
    {
        ArgumentNullException.ThrowIfNull(code);

        if (code.Length <= SeparatorPosition)
        {
            return code;
        }

        string separatorText = Separator.ToString();
        return code.Insert(SeparatorPosition, separatorText);
    }
}
