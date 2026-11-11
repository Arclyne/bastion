using System;

namespace Bastion.Server.Application.Chat;

public sealed record ChatHistoryEntry
{
    public ChatHistoryEntry(PostedChatLine postedLine, bool isOwn)
    {
        ArgumentNullException.ThrowIfNull(postedLine);

        PostedLine = postedLine;
        IsOwn = isOwn;
    }

    public PostedChatLine PostedLine { get; }

    public bool IsOwn { get; }
}
