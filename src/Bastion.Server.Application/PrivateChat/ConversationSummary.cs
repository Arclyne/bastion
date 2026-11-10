using System;

namespace Bastion.Server.Application.PrivateChat;

public sealed record ConversationSummary
{
    public ConversationSummary(ConversationPartner partner, PrivateMessageEntry lastMessage, int unreadCount)
    {
        ArgumentNullException.ThrowIfNull(partner);
        ArgumentNullException.ThrowIfNull(lastMessage);

        Partner = partner;
        LastMessage = lastMessage;
        UnreadCount = unreadCount;
    }

    public ConversationPartner Partner { get; }

    public PrivateMessageEntry LastMessage { get; }

    public int UnreadCount { get; }
}
