using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Chat;
using Bastion.Server.Application.PrivateChat;
using Microsoft.EntityFrameworkCore;

namespace Bastion.Server.Adapters.Persistence.Repositories;

public sealed class MessageRepository : IMessageRepository
{
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
        var latestFirst = SelectPrivateMessages()
            .Where(message => (message.AuthorId == accountId && message.RecipientId == partnerId)
                || (message.AuthorId == partnerId && message.RecipientId == accountId))
            .OrderByDescending(message => message.SentAt)
            .ThenByDescending(message => message.MessageId)
            .Take(count)
            .ToList();
        latestFirst.Reverse();
        return latestFirst;
    }

    public IReadOnlyList<Message> ListLatestPerPartner(int accountId)
    {
        IQueryable<long> latestMessageIds = SelectPrivateMessages()
            .Where(message => message.AuthorId == accountId || message.RecipientId == accountId)
            .GroupBy(message => message.AuthorId == accountId ? message.RecipientId : message.AuthorId)
            .Select(conversation => conversation.Max(message => message.MessageId));

        return _context.Messages
            .AsNoTracking()
            .Where(message => latestMessageIds.Contains(message.MessageId))
            .ToList();
    }

    public IReadOnlyDictionary<int, int> CountUnreadByAuthor(int recipientId)
    {
        return SelectPrivateMessages()
            .Where(message => message.RecipientId == recipientId && message.ReadAt == null)
            .GroupBy(message => message.AuthorId)
            .Select(unread => new { AuthorId = unread.Key, Count = unread.Count() })
            .ToDictionary(unread => unread.AuthorId, unread => unread.Count);
    }

    public void MarkRead(MessageParticipants readMessages, DateTime readAt)
    {
        ArgumentNullException.ThrowIfNull(readMessages);

        SelectPrivateMessages()
            .Where(message => message.AuthorId == readMessages.AuthorId
                && message.RecipientId == readMessages.RecipientId
                && message.ReadAt == null)
            .ExecuteUpdate(setters => setters.SetProperty(message => message.ReadAt, readAt));
    }

    private IQueryable<Message> SelectPrivateMessages()
    {
        return _context.Messages.AsNoTracking().Where(message => message.Channel == MessageChannel.Friends);
    }
}
