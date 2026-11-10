using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Chat;
using Bastion.Server.Application.PrivateChat;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeMessageRepository : IMessageRepository
{
    public List<Message> Messages { get; } = new List<Message>();

    public void Add(Message message)
    {
        ArgumentNullException.ThrowIfNull(message);

        Messages.Add(message);
    }

    public IReadOnlyList<Message> ListConversation(int accountId, int partnerId, int count)
    {
        return Messages
            .Where(message => message.GetPartnerId(accountId) == partnerId && Involves(message, accountId))
            .OrderBy(message => message.SentAt)
            .TakeLast(count)
            .ToList();
    }

    public IReadOnlyList<Message> ListLatestPerPartner(int accountId)
    {
        return Messages
            .Where(message => Involves(message, accountId))
            .GroupBy(message => message.GetPartnerId(accountId))
            .Select(conversation => conversation.OrderBy(message => message.SentAt).Last())
            .ToList();
    }

    public IReadOnlyDictionary<int, int> CountUnreadByAuthor(int recipientId)
    {
        return Messages
            .Where(message => message.RecipientId == recipientId && message.ReadAt is null)
            .GroupBy(message => message.AuthorId)
            .ToDictionary(unread => unread.Key, unread => unread.Count());
    }

    public void MarkRead(MessageParticipants participants, DateTime readAt)
    {
        ArgumentNullException.ThrowIfNull(participants);

        foreach (Message message in Messages.Where(message =>
            message.AuthorId == participants.AuthorId && message.RecipientId == participants.RecipientId))
        {
            message.MarkRead(readAt);
        }
    }

    private static bool Involves(Message message, int accountId)
    {
        return message.AuthorId == accountId || message.RecipientId == accountId;
    }
}
