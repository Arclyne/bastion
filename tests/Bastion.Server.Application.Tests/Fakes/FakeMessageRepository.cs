using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Chat;
using Bastion.Server.Application.PrivateChat;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeMessageRepository : IMessageRepository
{
    public FakeMessageRepository()
    {
        Messages = [];
    }

    public List<Message> Messages { get; }

    public void Add(Message message)
    {
        Messages.Add(message);
    }

    public IReadOnlyList<Message> ListConversation(int accountId, int partnerId, int count)
    {
        IEnumerable<Message> conversationMessages = Messages
            .Where(message => message.GetPartnerId(accountId) == partnerId && IsInvolving(message, accountId))
            .OrderBy(message => message.SentAt)
            .TakeLast(count);
        return new List<Message>(conversationMessages);
    }

    public IReadOnlyList<Message> ListLatestPerPartner(int accountId)
    {
        IEnumerable<Message> latestMessages = Messages
            .Where(message => IsInvolving(message, accountId))
            .GroupBy(message => message.GetPartnerId(accountId))
            .Select(FindLatest);
        return new List<Message>(latestMessages);
    }

    public IReadOnlyDictionary<int, int> CountUnreadByAuthor(int recipientId)
    {
        IEnumerable<KeyValuePair<int, int>> unreadCounts = Messages
            .Where(message => message.RecipientId == recipientId && message.ReadAt is null)
            .GroupBy(message => message.AuthorId)
            .Select(unreadMessages => new KeyValuePair<int, int>(unreadMessages.Key, unreadMessages.Count()));
        return new Dictionary<int, int>(unreadCounts);
    }

    public void MarkRead(MessageRoute route, DateTime readAt)
    {
        IEnumerable<Message> routeMessages = Messages.Where(message => IsFrom(message, route));
        foreach (Message message in routeMessages)
        {
            message.MarkRead(readAt);
        }
    }

    private static bool IsInvolving(Message message, int accountId)
    {
        return message.AuthorId == accountId || message.RecipientId == accountId;
    }

    private static Message FindLatest(IGrouping<int, Message> conversationMessages)
    {
        IEnumerable<Message> messagesInOrder = conversationMessages.OrderBy(message => message.SentAt);
        return messagesInOrder.Last();
    }

    private static bool IsFrom(Message message, MessageRoute route)
    {
        return message.AuthorId == route.AuthorId && message.RecipientId == route.RecipientId;
    }
}
