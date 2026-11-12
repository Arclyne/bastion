using System;

namespace Bastion.Client.Networking.PrivateChat;

public sealed record ConversationPreview
{
    public ConversationPreview(ConversationContact contact, PrivateMessage lastMessage, int unreadCount)
    {
        ArgumentNullException.ThrowIfNull(contact);
        ArgumentNullException.ThrowIfNull(lastMessage);

        Contact = contact;
        LastMessage = lastMessage;
        UnreadCount = unreadCount;
    }

    public ConversationContact Contact { get; }

    public PrivateMessage LastMessage { get; }

    public int UnreadCount { get; }
}
