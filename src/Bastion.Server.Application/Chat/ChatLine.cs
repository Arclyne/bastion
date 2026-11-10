using System;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Chat;

public sealed record ChatLine
{
    public ChatLine(ConnectedPlayer sender, string text, DateTime sentAt)
    {
        ArgumentNullException.ThrowIfNull(sender);
        ArgumentNullException.ThrowIfNull(text);

        Sender = sender;
        Text = text;
        SentAt = sentAt;
    }

    public ConnectedPlayer Sender { get; }

    public string Text { get; }

    public DateTime SentAt { get; }
}
