using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Chat;
using Bastion.Domain.Friends;
using Bastion.Server.Application.Chat;
using Bastion.Server.Application.Common;
using Bastion.Server.Application.Friends;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.PrivateChat;

public sealed class PrivateChatService : IPrivateChatService
{
    private readonly ISessionVerifier _sessionVerifier;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPrivateChatRoom _chatRoom;
    private readonly ChatModerator _moderator;

    public PrivateChatService(ISessionVerifier sessionVerifier, IUnitOfWork unitOfWork, IPrivateChatRoom chatRoom)
    {
        ArgumentNullException.ThrowIfNull(sessionVerifier);
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(chatRoom);

        _sessionVerifier = sessionVerifier;
        _unitOfWork = unitOfWork;
        _chatRoom = chatRoom;
        _moderator = new ChatModerator(unitOfWork);
    }

    public IReadOnlyList<ConversationSummary> ListConversations(string sessionToken, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        ConnectedPlayer player = _sessionVerifier.Verify(sessionToken, now);
        IReadOnlyList<Message> latestMessages = _unitOfWork.Messages.ListLatestPerPartner(player.AccountId);
        IReadOnlyDictionary<int, int> unreadByAuthor = _unitOfWork.Messages.CountUnreadByAuthor(player.AccountId);
        var friendIds = new HashSet<int>(_unitOfWork.Friendships.ListFriendAccountIds(player.AccountId));
        var partners = _unitOfWork.Accounts
            .ListPlayerIdentities(latestMessages.Select(message => message.GetPartnerId(player.AccountId)).ToList())
            .ToDictionary(identity => identity.AccountId);

        return latestMessages
            .Where(message => partners.ContainsKey(message.GetPartnerId(player.AccountId)))
            .Select(message =>
            {
                PlayerIdentity partner = partners[message.GetPartnerId(player.AccountId)];
                return new ConversationSummary(
                    new ConversationPartner(partner, friendIds.Contains(partner.AccountId)),
                    new PrivateMessageEntry(message, message.IsSentBy(player.AccountId)),
                    unreadByAuthor.GetValueOrDefault(partner.AccountId));
            })
            .OrderByDescending(summary => summary.LastMessage.Message.SentAt)
            .ToList();
    }

    public PrivateConversation OpenConversation(string sessionToken, string friendCode, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(friendCode);

        ConnectedPlayer player = _sessionVerifier.Verify(sessionToken, now);
        PlayerIdentity partner = FindPartner(player, friendCode);
        bool isFriend = IsFriend(player, partner);
        IReadOnlyList<Message> messages = _unitOfWork.Messages.ListConversation(
            player.AccountId,
            partner.AccountId,
            ChatRules.RecentMessageCount);
        if (!isFriend && messages.Count == 0)
        {
            throw new ChatMessageRejectedException(ChatRejection.NotFriends);
        }

        _unitOfWork.Messages.MarkRead(new MessageParticipants(partner.AccountId, player.AccountId), now);
        var entries = messages
            .Select(message => new PrivateMessageEntry(message, message.IsSentBy(player.AccountId)))
            .ToList();
        return new PrivateConversation(
            new ConversationPartner(partner, isFriend),
            entries,
            _moderator.FindMute(player.AccountId, now));
    }

    public Message SendPrivateMessage(string sessionToken, PrivateMessageDraft draft, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(draft);

        ConnectedPlayer player = _sessionVerifier.Verify(sessionToken, now);
        PlayerIdentity partner = FindPartner(player, draft.FriendCode);
        if (!IsFriend(player, partner))
        {
            throw new ChatMessageRejectedException(ChatRejection.NotFriends);
        }

        string screenedText = _moderator.Screen(player.AccountId, draft.Text, now);
        if (!_chatRoom.TryClaimTurn(player.AccountId, now))
        {
            throw new ChatMessageRejectedException(ChatRejection.TooFast);
        }

        PlayerIdentity author = _unitOfWork.Accounts.FindPlayerIdentity(player.AccountId)
            ?? throw new InvalidSessionException();
        var message = new Message(new MessageParticipants(player.AccountId, partner.AccountId), screenedText, now);
        _unitOfWork.Messages.Add(message);
        _unitOfWork.SaveChanges();
        _chatRoom.Deliver(new PrivateMessageDelivery(author, message));
        return message;
    }

    public void MarkConversationRead(string sessionToken, string friendCode, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(friendCode);

        ConnectedPlayer player = _sessionVerifier.Verify(sessionToken, now);
        PlayerIdentity partner = FindPartner(player, friendCode);
        _unitOfWork.Messages.MarkRead(new MessageParticipants(partner.AccountId, player.AccountId), now);
    }

    private PlayerIdentity FindPartner(ConnectedPlayer player, string friendCode)
    {
        string normalizedCode = FriendCodeRules.NormalizeFriendCode(friendCode);
        PlayerIdentity? partner = FriendCodeRules.IsValidFriendCode(normalizedCode)
            ? _unitOfWork.Accounts.FindPlayerIdentityByFriendCode(normalizedCode)
            : null;
        if (partner is null || partner.AccountId == player.AccountId)
        {
            throw new ChatMessageRejectedException(ChatRejection.PlayerNotFound);
        }

        return partner;
    }

    private bool IsFriend(ConnectedPlayer player, PlayerIdentity partner)
    {
        return _unitOfWork.Friendships.ListFriendAccountIds(player.AccountId).Contains(partner.AccountId);
    }
}
