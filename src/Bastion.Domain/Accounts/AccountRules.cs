using System;
using System.Text.RegularExpressions;

namespace Bastion.Domain.Accounts;

public static class AccountRules
{
    public const int MinimumNicknameLength = 3;
    public const int MaximumNicknameLength = 30;
    public const int MaximumEmailLength = 254;
    public const int MaximumIdentifierLength = MaximumEmailLength;

    private const int MatchTimeoutMilliseconds = 250;

    private static readonly TimeSpan _matchTimeout = TimeSpan.FromMilliseconds(MatchTimeoutMilliseconds);

    private static readonly Regex _nicknamePattern = new Regex(
        $"^[A-Za-z0-9_]{{{MinimumNicknameLength},{MaximumNicknameLength}}}$",
        RegexOptions.CultureInvariant,
        _matchTimeout);

    private static readonly Regex _emailPattern = new Regex(
        @"^[^@\s]+@[^@\s.]+(?:\.[^@\s.]+)+$",
        RegexOptions.CultureInvariant,
        _matchTimeout);

    public static bool IsNickname(string? text)
    {
        return text is not null && _nicknamePattern.IsMatch(text);
    }

    public static bool IsEmail(string? text)
    {
        return text is not null && text.Length <= MaximumEmailLength && _emailPattern.IsMatch(text);
    }

    public static bool IsIdentifier(string? text)
    {
        return IsNickname(text) || IsEmail(text);
    }
}
