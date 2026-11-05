using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Bastion.Domain.Friends;

public static class FriendCodeRules
{
    public const int Length = 6;

    private const int GroupLength = 2;
    private const char Separator = '-';
    private const string Alphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";
    private const int MatchTimeoutMilliseconds = 250;

    private static readonly TimeSpan _matchTimeout = TimeSpan.FromMilliseconds(MatchTimeoutMilliseconds);

    private static readonly Regex _friendCodePattern = new Regex(
        $"^[{Alphabet}]{{{Length}}}$",
        RegexOptions.CultureInvariant,
        _matchTimeout);

    public static bool IsValidFriendCode(string? text)
    {
        return text is not null && _friendCodePattern.IsMatch(text);
    }

    public static string NormalizeFriendCode(string text)
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

    public static string FormatFriendCode(string code)
    {
        ArgumentNullException.ThrowIfNull(code);

        if (code.Length <= GroupLength)
        {
            return code;
        }

        return code.Insert(GroupLength, Separator.ToString());
    }
}
