using System;
using Bastion.Client.Networking.Chat;

namespace Bastion.Client.Networking.Notifications;

public sealed class MatchChatMessageReceivedEventArgs : EventArgs
{
    public MatchChatMessageReceivedEventArgs(int matchId, ChatMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        MatchId = matchId;
        Message = message;
    }

    public int MatchId { get; }

    public ChatMessage Message { get; }
}
