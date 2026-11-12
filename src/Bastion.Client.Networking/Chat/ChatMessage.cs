using System;

namespace Bastion.Client.Networking.Chat;

public sealed record ChatMessage
{
    public ChatMessage(ChatAuthor author, string text, DateTime sentAt)
    {
        ArgumentNullException.ThrowIfNull(author);
        ArgumentNullException.ThrowIfNull(text);

        Author = author;
        Text = text;
        SentAt = sentAt;
    }

    public ChatAuthor Author { get; }

    public string Text { get; }

    public DateTime SentAt { get; }
}
