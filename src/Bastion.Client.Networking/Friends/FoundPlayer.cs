using System;

namespace Bastion.Client.Networking.Friends;

public sealed record FoundPlayer
{
    public FoundPlayer(PlayerTag player, FriendRelationship relationship)
    {
        ArgumentNullException.ThrowIfNull(player);

        Player = player;
        Relationship = relationship;
    }

    public PlayerTag Player { get; }

    public FriendRelationship Relationship { get; }
}
