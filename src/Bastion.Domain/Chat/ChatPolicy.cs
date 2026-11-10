namespace Bastion.Domain.Chat;

public static class ChatPolicy
{
    public const int MaximumMessageLength = 200;
    public const int MessageCooldownSeconds = 3;
    public const int MaximumRecentMessages = 50;

    public static bool IsValidMessage(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        string trimmedText = text.Trim();
        return trimmedText.Length <= MaximumMessageLength;
    }
}
