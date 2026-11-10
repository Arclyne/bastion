using System;
using Bastion.Server.Application.Chat;

namespace Bastion.Server.Application.PrivateChat;

public sealed record PrivateChatChannel
{
    public PrivateChatChannel(IPrivateChatRoom room, IChatModerator moderator)
    {
        ArgumentNullException.ThrowIfNull(room);
        ArgumentNullException.ThrowIfNull(moderator);

        Room = room;
        Moderator = moderator;
    }

    public IPrivateChatRoom Room { get; }

    public IChatModerator Moderator { get; }
}
