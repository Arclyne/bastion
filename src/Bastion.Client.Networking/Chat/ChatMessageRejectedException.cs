using System;

namespace Bastion.Client.Networking.Chat;

public sealed class ChatMessageRejectedException : Exception
{
    public ChatMessageRejectedException(ChatRejection rejection, DateTime? mutedUntil, Exception innerException)
        : base($"The chat message was rejected. Rejection={rejection}", innerException)
    {
        Rejection = rejection;
        MutedUntil = mutedUntil;
    }

    public ChatRejection Rejection { get; }

    public DateTime? MutedUntil { get; }
}
