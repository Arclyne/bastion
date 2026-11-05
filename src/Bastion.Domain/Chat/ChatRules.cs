namespace Bastion.Domain.Chat;

public static class ChatRules
{
    public const int MaximumMessageLength = 200;
    public const int CooldownSeconds = 3;
    public const int HistoryCapacity = 200;
    public const int RecentMessageCount = 50;

    public static bool IsValidMessage(string? text)
    {
        return !string.IsNullOrWhiteSpace(text) && text.Trim().Length <= MaximumMessageLength;
    }
}
