using System;
using Bastion.Client.Networking.Friends;

namespace Bastion.Client.Networking.PrivateChat;

public sealed record ConversationContact
{
    public ConversationContact(PlayerTag partner, bool isFriend)
    {
        ArgumentNullException.ThrowIfNull(partner);

        Partner = partner;
        IsFriend = isFriend;
    }

    public PlayerTag Partner { get; }

    public bool IsFriend { get; }
}
