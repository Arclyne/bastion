using System;
using System.Collections.Generic;

namespace Bastion.Server.Application.Chat;

public sealed record LobbyChatHistory
{
    public LobbyChatHistory(IReadOnlyList<ChatHistoryEntry> entries, int connectedPlayerCount, ChatMute mute)
    {
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentNullException.ThrowIfNull(mute);

        Entries = entries;
        ConnectedPlayerCount = connectedPlayerCount;
        Mute = mute;
    }

    public IReadOnlyList<ChatHistoryEntry> Entries { get; }

    public int ConnectedPlayerCount { get; }

    public ChatMute Mute { get; }
}
