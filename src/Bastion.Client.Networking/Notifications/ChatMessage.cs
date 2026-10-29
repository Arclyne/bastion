using System;

namespace Bastion.Client.Networking.Notifications;

public sealed class ChatMessage
{
    public ChatMessage(string senderNickname, string text, DateTime sentAt)
    {
        ArgumentNullException.ThrowIfNull(senderNickname);
        ArgumentNullException.ThrowIfNull(text);

        SenderNickname = senderNickname;
        Text = text;
        SentAt = sentAt;
    }

    public string SenderNickname { get; }

    public string Text { get; }

    public DateTime SentAt { get; }
}
