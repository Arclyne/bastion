using System;

namespace Bastion.Server.Application.Chat;

public sealed class ChatMessageRejectedException : Exception
{
    public ChatMessageRejectedException(ChatRejection rejection)
        : base($"The chat message was rejected. Rejection={rejection}")
    {
        Rejection = rejection;
    }

    public ChatMessageRejectedException(ChatMute mute)
        : this(ChatRejection.Muted)
    {
        ArgumentNullException.ThrowIfNull(mute);

        MutedUntil = mute.EndsAt;
    }

    public ChatRejection Rejection { get; }

    public DateTime? MutedUntil { get; }
}
