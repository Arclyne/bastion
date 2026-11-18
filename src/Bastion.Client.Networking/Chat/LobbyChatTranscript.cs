using System;
using System.Collections.Generic;

namespace Bastion.Client.Networking.Chat;

public sealed record LobbyChatTranscript
{
    public LobbyChatTranscript(IReadOnlyList<ChatMessage> messages, int connectedPlayerCount, ChatMuteStatus muteStatus)
    {
        ArgumentNullException.ThrowIfNull(messages);
        ArgumentNullException.ThrowIfNull(muteStatus);

        Messages = messages;
        ConnectedPlayerCount = connectedPlayerCount;
        MuteStatus = muteStatus;
    }

    public IReadOnlyList<ChatMessage> Messages { get; }

    public int ConnectedPlayerCount { get; }

    public ChatMuteStatus MuteStatus { get; }
}
