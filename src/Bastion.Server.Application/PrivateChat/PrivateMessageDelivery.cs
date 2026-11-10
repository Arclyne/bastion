using System;
using Bastion.Domain.Chat;
using Bastion.Server.Application.Accounts;

namespace Bastion.Server.Application.PrivateChat;

public sealed record PrivateMessageDelivery
{
    public PrivateMessageDelivery(PlayerIdentity author, Message message)
    {
        ArgumentNullException.ThrowIfNull(author);
        ArgumentNullException.ThrowIfNull(message);

        Author = author;
        Message = message;
    }

    public PlayerIdentity Author { get; }

    public Message Message { get; }
}
