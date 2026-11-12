using System;

namespace Bastion.Client.Networking.PrivateChat;

public sealed record PrivateMessage
{
    public PrivateMessage(string text, DateTime sentAt, bool isOwn)
    {
        ArgumentNullException.ThrowIfNull(text);

        Text = text;
        SentAt = sentAt;
        IsOwn = isOwn;
    }

    public string Text { get; }

    public DateTime SentAt { get; init; }

    public bool IsOwn { get; }
}
