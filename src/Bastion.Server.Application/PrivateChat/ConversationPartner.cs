using System;
using Bastion.Server.Application.Accounts;

namespace Bastion.Server.Application.PrivateChat;

public sealed record ConversationPartner
{
    public ConversationPartner(PlayerIdentity player, bool isFriend)
    {
        ArgumentNullException.ThrowIfNull(player);

        Player = player;
        IsFriend = isFriend;
    }

    public PlayerIdentity Player { get; }

    public bool IsFriend { get; }
}
