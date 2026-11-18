using System;
using System.Collections.Generic;

namespace Bastion.Server.Application.Chat;

public sealed record LobbyChatHistory
{
    public LobbyChatHistory(
        IReadOnlyList<ChatHistoryEntry> entries,
        int connectedPlayerCount,
        ChatMuteStatus muteStatus)
    {
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentNullException.ThrowIfNull(muteStatus);

        Entries = entries;
        ConnectedPlayerCount = connectedPlayerCount;
        MuteStatus = muteStatus;
    }

    public IReadOnlyList<ChatHistoryEntry> Entries { get; }

    public int ConnectedPlayerCount { get; }

    public ChatMuteStatus MuteStatus { get; }
}
