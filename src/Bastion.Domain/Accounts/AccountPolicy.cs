using System.Text.RegularExpressions;
using Bastion.Domain.Text;

namespace Bastion.Domain.Accounts;

public static class AccountPolicy
{
    public const int MinimumNicknameLength = 3;
    public const int MaximumNicknameLength = 30;
    public const int MaximumEmailLength = 254;
    public const int MaximumLoginIdentifierLength = MaximumEmailLength;

    private static readonly Regex _nicknameCharacters = TextPattern.Create("^[A-Za-z0-9_]+$");

    private static readonly Regex _emailPattern = TextPattern.Create(@"^[^@\s]+@[^@\s.]+(?:\.[^@\s.]+)+$");

    public static bool IsValidNickname(string? nickname)
    {
        if (nickname is null || nickname.Length < MinimumNicknameLength || nickname.Length > MaximumNicknameLength)
        {
            return false;
        }

        return _nicknameCharacters.IsMatch(nickname);
    }

    public static bool IsValidEmail(string? email)
    {
        if (email is null || email.Length > MaximumEmailLength)
        {
            return false;
        }

        return _emailPattern.IsMatch(email);
    }

    public static bool IsValidLoginIdentifier(string? identifier)
    {
        return IsValidNickname(identifier) || IsValidEmail(identifier);
    }
}
