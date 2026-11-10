using System;
using Bastion.Server.Application.Accounts;

namespace Bastion.Server.Application.Friends;

public sealed record PlayerSearchResult
{
    public PlayerSearchResult(PlayerIdentity player, FriendRelationship relationship)
    {
        ArgumentNullException.ThrowIfNull(player);

        Player = player;
        Relationship = relationship;
    }

    public PlayerIdentity Player { get; }

    public FriendRelationship Relationship { get; }
}
