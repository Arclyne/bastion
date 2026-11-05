using System;
using System.Linq;
using Bastion.Domain.Chat;
using Bastion.Server.Application.Common;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Chat;

public sealed class LobbyChatService : ILobbyChatService
{
    private readonly ISessionVerifier _sessionVerifier;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILobbyChatRoom _chatRoom;

    public LobbyChatService(ISessionVerifier sessionVerifier, IUnitOfWork unitOfWork, ILobbyChatRoom chatRoom)
    {
        ArgumentNullException.ThrowIfNull(sessionVerifier);
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(chatRoom);

        _sessionVerifier = sessionVerifier;
        _unitOfWork = unitOfWork;
        _chatRoom = chatRoom;
    }

    public PostedChatLine SendMessage(string sessionToken, string text, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(text);

        ConnectedPlayer player = _sessionVerifier.Verify(sessionToken, now);
        if (!ChatRules.IsValidMessage(text))
        {
            throw new ChatMessageRejectedException(ChatRejection.InvalidMessage);
        }

        ChatMute mute = FindMute(player.AccountId, now);
        if (mute.IsMuted)
        {
            throw new ChatMessageRejectedException(mute);
        }

        string trimmedText = text.Trim();
        if (ChatWordFilter.ContainsBannedTerm(trimmedText, _unitOfWork.BannedWords.ListChatTerms()))
        {
            throw new ChatMessageRejectedException(ChatRejection.BannedTerm);
        }

        if (!_chatRoom.TryClaimTurn(player.AccountId, now))
        {
            throw new ChatMessageRejectedException(ChatRejection.TooFast);
        }

        return _chatRoom.Post(new ChatLine(player, trimmedText, now));
    }

    public LobbyChatHistory OpenLobbyChat(string sessionToken, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        ConnectedPlayer player = _sessionVerifier.Verify(sessionToken, now);
        var entries = _chatRoom.ListRecent(ChatRules.RecentMessageCount)
            .Select(line => new ChatHistoryEntry(line, line.Line.Sender.AccountId == player.AccountId))
            .ToList();
        return new LobbyChatHistory(entries, _chatRoom.CountListeners(), FindMute(player.AccountId, now));
    }

    private ChatMute FindMute(int accountId, DateTime now)
    {
        var sanctionsInEffect = _unitOfWork.Sanctions.ListChatSanctions(accountId)
            .Where(sanction => sanction.IsInEffectAt(now))
            .ToList();
        return ChatMute.FromSanctionsInEffect(sanctionsInEffect);
    }
}
