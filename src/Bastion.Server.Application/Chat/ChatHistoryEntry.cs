using System;

namespace Bastion.Server.Application.Chat;

public sealed record ChatHistoryEntry
{
    public ChatHistoryEntry(PostedChatLine line, bool isOwn)
    {
        ArgumentNullException.ThrowIfNull(line);

        Line = line;
        IsOwn = isOwn;
    }

    public PostedChatLine Line { get; }

    public bool IsOwn { get; }
}
