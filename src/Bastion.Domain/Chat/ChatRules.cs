namespace Bastion.Domain.Chat;

public static class ChatRules
{
    public const int MaximumMessageLength = 200;

    public static bool IsMessage(string? text)
    {
        return !string.IsNullOrWhiteSpace(text) && text.Trim().Length <= MaximumMessageLength;
    }
}
