using System;

namespace Bastion.Server.Application.Friends;

public sealed record FriendStatus
{
    public FriendStatus(FriendSummary summary, FriendPresence presence)
    {
        ArgumentNullException.ThrowIfNull(summary);

        Summary = summary;
        Presence = presence;
    }

    public FriendSummary Summary { get; }

    public FriendPresence Presence { get; }
}
