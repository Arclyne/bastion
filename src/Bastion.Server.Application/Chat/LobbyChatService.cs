using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Chat;

/// <inheritdoc/>
public sealed class LobbyChatService : ILobbyChatService
{
    private readonly ISessionVerifier _sessionVerifier;
    private readonly IChatModerator _chatModerator;
    private readonly ILobbyChatRoom _chatRoom;

    public LobbyChatService(ISessionVerifier sessionVerifier, IChatModerator chatModerator, ILobbyChatRoom chatRoom)
    {
        ArgumentNullException.ThrowIfNull(sessionVerifier);
        ArgumentNullException.ThrowIfNull(chatModerator);
        ArgumentNullException.ThrowIfNull(chatRoom);

        _sessionVerifier = sessionVerifier;
        _chatModerator = chatModerator;
        _chatRoom = chatRoom;
    }

    /// <inheritdoc/>
    public PostedChatLine SendMessage(string sessionToken, string text, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(text);

        ConnectedPlayer player = _sessionVerifier.Verify(sessionToken, now);
        string screenedText = _chatModerator.ScreenMessage(player.AccountId, text, now);
        if (!_chatRoom.TryStartCooldown(player.AccountId, now))
        {
            throw new ChatMessageRejectedException(ChatRejection.TooFast);
        }

        var line = new ChatLine(player, screenedText, now);
        return _chatRoom.Post(line);
    }

    /// <inheritdoc/>
    public LobbyChatHistory OpenLobbyChat(string sessionToken, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        ConnectedPlayer player = _sessionVerifier.Verify(sessionToken, now);
        IReadOnlyList<PostedChatLine> recentLines = _chatRoom.ListRecent();
        IEnumerable<ChatHistoryEntry> entries = recentLines
            .Select(postedLine => new ChatHistoryEntry(postedLine, postedLine.Line.Sender.AccountId == player.AccountId));
        var entryList = new List<ChatHistoryEntry>(entries);
        int connectedPlayerCount = _chatRoom.CountListeners();
        ChatMute mute = _chatModerator.FindMute(player.AccountId, now);
        return new LobbyChatHistory(entryList, connectedPlayerCount, mute);
    }
}
