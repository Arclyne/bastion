using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Bastion.Domain.Chat;
using Bastion.Server.Application.PrivateChat;
using Microsoft.EntityFrameworkCore;

namespace Bastion.Server.Adapters.Persistence.Repositories;

public sealed class MessageRepository : IMessageRepository
{
    private static readonly Expression<Func<Message, DateTime?>> _readAt = message => message.ReadAt;

    private readonly BastionDbContext _context;

    public MessageRepository(BastionDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;
    }

    public void Add(Message message)
    {
        ArgumentNullException.ThrowIfNull(message);

        _context.Messages.Add(message);
    }

    public IReadOnlyList<Message> ListConversation(int accountId, int partnerId, int count)
    {
        IQueryable<Message> sentMessages = SelectMessagesFrom(accountId, partnerId);
        IQueryable<Message> receivedMessages = SelectMessagesFrom(partnerId, accountId);
        IQueryable<Message> latestMessages = sentMessages
            .Concat(receivedMessages)
            .OrderByDescending(message => message.SentAt)
            .ThenByDescending(message => message.MessageId)
            .Take(count);
        var conversationMessages = new List<Message>(latestMessages);
        conversationMessages.Reverse();
        return conversationMessages;
    }

    public IReadOnlyList<Message> ListLatestPerPartner(int accountId)
    {
        IQueryable<long> latestMessageIds = SelectPrivateMessages()
            .Where(message => message.AuthorId == accountId || message.RecipientId == accountId)
            .GroupBy(
                message => message.AuthorId == accountId ? message.RecipientId : message.AuthorId,
                message => message.MessageId)
            .Select(messageIds => messageIds.Max());
        IQueryable<Message> latestMessages = _context.Messages
            .AsNoTracking()
            .Where(message => latestMessageIds.Contains(message.MessageId));
        return new List<Message>(latestMessages);
    }

    public IReadOnlyDictionary<int, int> CountUnreadByAuthor(int recipientId)
    {
        IQueryable<KeyValuePair<int, int>> unreadCounts = SelectPrivateMessages()
            .Where(message => message.RecipientId == recipientId)
            .Where(message => message.ReadAt == null)
            .GroupBy(message => message.AuthorId)
            .Select(unread => new KeyValuePair<int, int>(unread.Key, unread.Count()));
        return new Dictionary<int, int>(unreadCounts);
    }

    public void MarkRead(MessageParticipants participants, DateTime readAt)
    {
        ArgumentNullException.ThrowIfNull(participants);

        IQueryable<Message> unreadMessages = SelectMessagesFrom(participants.AuthorId, participants.RecipientId)
            .Where(message => message.ReadAt == null);
        unreadMessages.ExecuteUpdate(setters => setters.SetProperty(_readAt, readAt));
    }

    private IQueryable<Message> SelectMessagesFrom(int authorId, int recipientId)
    {
        IQueryable<Message> sentMessages = SelectPrivateMessages()
            .Where(message => message.AuthorId == authorId)
            .Where(message => message.RecipientId == recipientId);
        return sentMessages;
    }

    private IQueryable<Message> SelectPrivateMessages()
    {
        IQueryable<Message> privateMessages = _context.Messages
            .AsNoTracking()
            .Where(message => message.Channel == MessageChannel.Friends);
        return privateMessages;
    }
}
