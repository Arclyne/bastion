using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Chat;
using Bastion.Server.Application.Accounts;
using Bastion.Server.Application.Chat;
using Bastion.Server.Application.Common;
using Bastion.Server.Application.Friends;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.PrivateChat;

/// <inheritdoc/>
public sealed class PrivateChatService : IPrivateChatService
{
    private readonly ISessionVerifier _sessionVerifier;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPrivateChatRoom _chatRoom;
    private readonly IChatModerator _chatModerator;

    public PrivateChatService(ISessionVerifier sessionVerifier, IUnitOfWork unitOfWork, PrivateChatChannel channel)
    {
        ArgumentNullException.ThrowIfNull(sessionVerifier);
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(channel);

        _sessionVerifier = sessionVerifier;
        _unitOfWork = unitOfWork;
        _chatRoom = channel.Room;
        _chatModerator = channel.Moderator;
    }

    /// <inheritdoc/>
    public IReadOnlyList<ConversationSummary> ListConversations(string sessionToken, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        ConnectedPlayer player = _sessionVerifier.Verify(sessionToken, now);
        IReadOnlyList<Message> latestMessages = _unitOfWork.Messages.ListLatestPerPartner(player.AccountId);
        IReadOnlyDictionary<int, int> unreadByAuthor = _unitOfWork.Messages.CountUnreadByAuthor(player.AccountId);
        var friendIds = new HashSet<int>(_unitOfWork.Friendships.ListFriendAccountIds(player.AccountId));
        IEnumerable<int> partnerIdSequence = latestMessages.Select(message => message.GetPartnerId(player.AccountId));
        var partnerIds = new List<int>(partnerIdSequence);
        IReadOnlyList<PlayerIdentity> partnerIdentities = _unitOfWork.Accounts.ListPlayerIdentities(partnerIds);
        var partnersById = new Dictionary<int, PlayerIdentity>(partnerIdentities.Count);
        foreach (PlayerIdentity identity in partnerIdentities)
        {
            partnersById.Add(identity.AccountId, identity);
        }

        var summaries = new List<ConversationSummary>();
        foreach (Message message in latestMessages)
        {
            int partnerId = message.GetPartnerId(player.AccountId);
            if (!partnersById.TryGetValue(partnerId, out PlayerIdentity? partner))
            {
                continue;
            }

            var conversationPartner = new ConversationPartner(partner, friendIds.Contains(partnerId));
            var lastMessage = new PrivateMessageEntry(message, message.IsSentBy(player.AccountId));
            int unreadCount = unreadByAuthor.GetValueOrDefault(partnerId);
            summaries.Add(new ConversationSummary(conversationPartner, lastMessage, unreadCount));
        }

        IEnumerable<ConversationSummary> latestFirst = summaries
            .OrderByDescending(summary => summary.LastMessage.Message.SentAt);
        return new List<ConversationSummary>(latestFirst);
    }

    /// <inheritdoc/>
    public PrivateConversation OpenConversation(string sessionToken, string friendCode, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(friendCode);

        ConnectedPlayer player = _sessionVerifier.Verify(sessionToken, now);
        PlayerIdentity partner = FindPartner(player.AccountId, friendCode);
        bool isFriend = IsFriend(player.AccountId, partner.AccountId);
        IReadOnlyList<Message> messages = _unitOfWork.Messages.ListConversation(
            player.AccountId,
            partner.AccountId,
            ChatPolicy.MaximumRecentMessages);
        if (!isFriend && messages.Count == 0)
        {
            throw new ChatMessageRejectedException(ChatRejection.NotFriends);
        }

        _unitOfWork.Messages.MarkRead(new MessageParticipants(partner.AccountId, player.AccountId), now);
        IEnumerable<PrivateMessageEntry> entries = messages
            .Select(message => new PrivateMessageEntry(message, message.IsSentBy(player.AccountId)));
        var entryList = new List<PrivateMessageEntry>(entries);
        var conversationPartner = new ConversationPartner(partner, isFriend);
        ChatMute mute = _chatModerator.FindMute(player.AccountId, now);
        return new PrivateConversation(conversationPartner, entryList, mute);
    }

    /// <inheritdoc/>
    public Message SendPrivateMessage(string sessionToken, PrivateMessageDraft draft, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(draft);

        PlayerIdentity author = _sessionVerifier.VerifyIdentity(sessionToken, now);
        PlayerIdentity partner = FindPartner(author.AccountId, draft.FriendCode);
        if (!IsFriend(author.AccountId, partner.AccountId))
        {
            throw new ChatMessageRejectedException(ChatRejection.NotFriends);
        }

        string screenedText = _chatModerator.ScreenMessage(author.AccountId, draft.Text, now);
        if (!_chatRoom.TryStartCooldown(author.AccountId, now))
        {
            throw new ChatMessageRejectedException(ChatRejection.TooFast);
        }

        var participants = new MessageParticipants(author.AccountId, partner.AccountId);
        var message = Message.CreatePrivate(participants, screenedText, now);
        _unitOfWork.Messages.Add(message);
        _unitOfWork.SaveChanges();
        _chatRoom.Deliver(new PrivateMessageDelivery(author, message));
        return message;
    }

    /// <inheritdoc/>
    public void MarkConversationRead(string sessionToken, string friendCode, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(friendCode);

        ConnectedPlayer player = _sessionVerifier.Verify(sessionToken, now);
        PlayerIdentity partner = FindPartner(player.AccountId, friendCode);
        _unitOfWork.Messages.MarkRead(new MessageParticipants(partner.AccountId, player.AccountId), now);
    }

    private PlayerIdentity FindPartner(int accountId, string friendCode)
    {
        PlayerIdentity? partner = FriendCodeLookup.FindPlayer(_unitOfWork.Accounts, friendCode);
        if (partner is null || partner.AccountId == accountId)
        {
            throw new ChatMessageRejectedException(ChatRejection.PlayerNotFound);
        }

        return partner;
    }

    private bool IsFriend(int accountId, int partnerAccountId)
    {
        IReadOnlyList<int> friendAccountIds = _unitOfWork.Friendships.ListFriendAccountIds(accountId);
        return friendAccountIds.Contains(partnerAccountId);
    }
}
