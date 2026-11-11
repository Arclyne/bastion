using System;
using System.Collections.Generic;

namespace Bastion.Client.Networking.Chat;

public sealed record LobbyChatTranscript
{
    public LobbyChatTranscript(IReadOnlyList<ChatMessage> messages, int connectedPlayerCount, ChatMute mute)
    {
        ArgumentNullException.ThrowIfNull(messages);
        ArgumentNullException.ThrowIfNull(mute);

        Messages = messages;
        ConnectedPlayerCount = connectedPlayerCount;
        Mute = mute;
    }

    public IReadOnlyList<ChatMessage> Messages { get; }

    public int ConnectedPlayerCount { get; }

    public ChatMute Mute { get; }
}
