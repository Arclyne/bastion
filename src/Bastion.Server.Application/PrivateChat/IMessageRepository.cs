using System;
using System.Collections.Generic;
using Bastion.Domain.Chat;

namespace Bastion.Server.Application.PrivateChat;

public interface IMessageRepository
{
    void Add(Message message);

    IReadOnlyList<Message> ListConversation(int accountId, int partnerId, int count);

    IReadOnlyList<Message> ListLatestPerPartner(int accountId);

    IReadOnlyDictionary<int, int> CountUnreadByAuthor(int recipientId);

    void MarkRead(MessageParticipants readMessages, DateTime readAt);
}
