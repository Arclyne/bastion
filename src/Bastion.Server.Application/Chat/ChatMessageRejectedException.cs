using System;

namespace Bastion.Server.Application.Chat;

public sealed class ChatMessageRejectedException : Exception
{
    public ChatMessageRejectedException(ChatRejection rejection)
        : base($"The chat message was rejected. Rejection={rejection}")
    {
        Rejection = rejection;
    }

    public ChatMessageRejectedException(ChatMuteStatus muteStatus)
        : this(ChatRejection.Muted)
    {
        ArgumentNullException.ThrowIfNull(muteStatus);

        MutedUntil = muteStatus.EndsAt;
    }

    public ChatRejection Rejection { get; }

    public DateTime? MutedUntil { get; }
}
