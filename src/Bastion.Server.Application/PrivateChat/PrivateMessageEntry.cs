using System;
using Bastion.Domain.Chat;

namespace Bastion.Server.Application.PrivateChat;

public sealed record PrivateMessageEntry
{
    public PrivateMessageEntry(Message message, bool isOwn)
    {
        ArgumentNullException.ThrowIfNull(message);

        Message = message;
        IsOwn = isOwn;
    }

    public Message Message { get; }

    public bool IsOwn { get; }
}
