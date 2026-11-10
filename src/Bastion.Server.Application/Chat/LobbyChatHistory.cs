using System;
using System.Collections.Generic;

namespace Bastion.Server.Application.Chat;

public sealed record LobbyChatHistory
{
    public LobbyChatHistory(IReadOnlyList<ChatHistoryEntry> entries, int connectedPlayers, ChatMute mute)
    {
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentNullException.ThrowIfNull(mute);

        Entries = entries;
        ConnectedPlayers = connectedPlayers;
        Mute = mute;
    }

    public IReadOnlyList<ChatHistoryEntry> Entries { get; }

    public int ConnectedPlayers { get; }

    public ChatMute Mute { get; }
}
