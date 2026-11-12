using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Chat;
using Bastion.Domain.Moderation;
using Bastion.Server.Application.Chat;
using Bastion.Server.Application.Sessions;
using Bastion.Server.Application.Tests.Fakes;
using Xunit;

namespace Bastion.Server.Application.Tests.Chat;

public sealed class TestLobbyChatService
{
    private const int AccountId = 4;
    private const string Nickname = "marta_muros";
    private const string Token = "marta-token";
    private const int OtherAccountId = 3;
    private const string OtherNickname = "luis_quo";
    private const string OtherToken = "luis-token";
    private const string UnknownToken = "unknown-token";
    private const string BannedTerm = "tonto";
    private const int ListenerCount = 7;
    private const string CleanText = "hola";
    private const string PaddedText = "  hola  ";
    private const string BlankText = "   ";
    private const string ShoutedBannedText = "eres un TONTO";
    private const string RepeatedText = "otra vez";
    private const string ApologyText = "perdón";
    private const string FirstText = "primero";
    private const string SecondText = "segundo";
    private const char FillerCharacter = 'a';
    private const int SanctionDays = 1;
    private const int SecondsWithinCooldown = 1;
    private const int TwoBroadcastLines = 2;
    private const string ManyPlayersTokenPrefix = "player-";
    private const string ManyPlayersNicknamePrefix = "player_";

    private static readonly DateTime _now = new DateTime(2026, 11, 5, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _withinCooldown = _now.AddSeconds(SecondsWithinCooldown);
    private static readonly ConnectedPlayer _player = new ConnectedPlayer(AccountId, Nickname);
    private static readonly ConnectedPlayer _otherPlayer = new ConnectedPlayer(OtherAccountId, OtherNickname);

    private readonly FakeUnitOfWork _unitOfWork = new FakeUnitOfWork();
    private readonly FakeSessionVerifier _sessionVerifier = new FakeSessionVerifier();
    private readonly FakeChatBroadcaster _broadcaster = new FakeChatBroadcaster();
    private readonly LobbyChatService _lobbyChatService;

    public TestLobbyChatService()
    {
        var moderator = new ChatModerator(_unitOfWork);
        var chatRoom = new LobbyChatRoom(_broadcaster);
        _lobbyChatService = new LobbyChatService(_sessionVerifier, moderator, chatRoom);
    }

    [Fact]
    public void SendMessage_InvalidSession_ThrowsInvalidSession()
    {
        Action send = () => _lobbyChatService.SendMessage(UnknownToken, CleanText, _now);

        Assert.Throws<InvalidSessionException>(send);
    }

    [Fact]
    public void SendMessage_EmptyText_RejectsInvalidMessage()
    {
        _sessionVerifier.Add(Token, _player);
        Action send = () => _lobbyChatService.SendMessage(Token, BlankText, _now);

        Exception? exception = Record.Exception(send);

        var rejection = exception as ChatMessageRejectedException;
        Assert.Equal(ChatRejection.InvalidMessage, rejection?.Rejection);
    }

    [Fact]
    public void SendMessage_TooLongText_RejectsInvalidMessage()
    {
        _sessionVerifier.Add(Token, _player);
        int tooLongLength = ChatPolicy.MaximumMessageLength + 1;
        string tooLongText = new string(FillerCharacter, tooLongLength);
        Action send = () => _lobbyChatService.SendMessage(Token, tooLongText, _now);

        Exception? exception = Record.Exception(send);

        var rejection = exception as ChatMessageRejectedException;
        Assert.Equal(ChatRejection.InvalidMessage, rejection?.Rejection);
    }

    [Fact]
    public void SendMessage_ValidText_BroadcastsTrimmedText()
    {
        _sessionVerifier.Add(Token, _player);

        _lobbyChatService.SendMessage(Token, PaddedText, _now);

        PostedChatLine broadcastLine = _broadcaster.Lines[0];
        Assert.Equal(CleanText, broadcastLine.Line.Text);
    }

    [Fact]
    public void SendMessage_ValidText_BroadcastsSender()
    {
        _sessionVerifier.Add(Token, _player);

        _lobbyChatService.SendMessage(Token, CleanText, _now);

        PostedChatLine broadcastLine = _broadcaster.Lines[0];
        Assert.Equal(AccountId, broadcastLine.Line.Sender.AccountId);
    }

    [Fact]
    public void SendMessage_ValidText_ReturnsPostedLine()
    {
        _sessionVerifier.Add(Token, _player);

        PostedChatLine postedLine = _lobbyChatService.SendMessage(Token, CleanText, _now);

        PostedChatLine broadcastLine = _broadcaster.Lines[0];
        Assert.Equal(broadcastLine, postedLine);
    }

    [Fact]
    public void SendMessage_TemporaryChatSanction_RejectsMuted()
    {
        _sessionVerifier.Add(Token, _player);
        DateTime muteEnd = _now.AddDays(SanctionDays);
        AddSanction(SanctionScope.Chat, muteEnd);
        Action send = () => _lobbyChatService.SendMessage(Token, CleanText, _now);

        Exception? exception = Record.Exception(send);

        var rejection = exception as ChatMessageRejectedException;
        Assert.Equal(ChatRejection.Muted, rejection?.Rejection);
    }

    [Fact]
    public void SendMessage_TemporaryChatSanction_ReportsMuteEnd()
    {
        _sessionVerifier.Add(Token, _player);
        DateTime muteEnd = _now.AddDays(SanctionDays);
        AddSanction(SanctionScope.Chat, muteEnd);
        Action send = () => _lobbyChatService.SendMessage(Token, CleanText, _now);

        Exception? exception = Record.Exception(send);

        var rejection = exception as ChatMessageRejectedException;
        Assert.Equal(muteEnd, rejection?.MutedUntil);
    }

    [Fact]
    public void SendMessage_EndedChatSanction_BroadcastsMessage()
    {
        _sessionVerifier.Add(Token, _player);
        DateTime muteEnd = _now.AddDays(-SanctionDays);
        AddSanction(SanctionScope.Chat, muteEnd);

        _lobbyChatService.SendMessage(Token, CleanText, _now);

        Assert.Single(_broadcaster.Lines);
    }

    [Fact]
    public void SendMessage_AccountSanctionOnly_BroadcastsMessage()
    {
        _sessionVerifier.Add(Token, _player);
        AddSanction(SanctionScope.Account, endsAt: null);

        _lobbyChatService.SendMessage(Token, CleanText, _now);

        Assert.Single(_broadcaster.Lines);
    }

    [Fact]
    public void SendMessage_BannedTerm_RejectsBannedTerm()
    {
        _sessionVerifier.Add(Token, _player);
        _unitOfWork.BannedWordRepository.ChatTerms.Add(BannedTerm);
        Action send = () => _lobbyChatService.SendMessage(Token, ShoutedBannedText, _now);

        Exception? exception = Record.Exception(send);

        var rejection = exception as ChatMessageRejectedException;
        Assert.Equal(ChatRejection.BannedTerm, rejection?.Rejection);
    }

    [Fact]
    public void SendMessage_BannedTerm_DoesNotBroadcast()
    {
        _sessionVerifier.Add(Token, _player);
        _unitOfWork.BannedWordRepository.ChatTerms.Add(BannedTerm);
        Action send = () => _lobbyChatService.SendMessage(Token, BannedTerm, _now);

        Record.Exception(send);

        Assert.Empty(_broadcaster.Lines);
    }

    [Fact]
    public void SendMessage_SecondMessageWithinCooldown_RejectsTooFast()
    {
        _sessionVerifier.Add(Token, _player);
        _lobbyChatService.SendMessage(Token, CleanText, _now);
        Action sendAgain = () => _lobbyChatService.SendMessage(Token, RepeatedText, _withinCooldown);

        Exception? exception = Record.Exception(sendAgain);

        var rejection = exception as ChatMessageRejectedException;
        Assert.Equal(ChatRejection.TooFast, rejection?.Rejection);
    }

    [Fact]
    public void SendMessage_SecondMessageAfterCooldown_BroadcastsBoth()
    {
        _sessionVerifier.Add(Token, _player);
        _lobbyChatService.SendMessage(Token, CleanText, _now);
        DateTime cooldownEnd = _now.AddSeconds(ChatPolicy.MessageCooldownSeconds);

        _lobbyChatService.SendMessage(Token, RepeatedText, cooldownEnd);

        Assert.Equal(TwoBroadcastLines, _broadcaster.Lines.Count);
    }

    [Fact]
    public void SendMessage_AfterRejectedMessage_KeepsTurn()
    {
        _sessionVerifier.Add(Token, _player);
        _unitOfWork.BannedWordRepository.ChatTerms.Add(BannedTerm);
        Action sendBanned = () => _lobbyChatService.SendMessage(Token, BannedTerm, _now);
        Record.Exception(sendBanned);

        _lobbyChatService.SendMessage(Token, ApologyText, _withinCooldown);

        Assert.Single(_broadcaster.Lines);
    }

    [Fact]
    public void SendMessage_OtherPlayerWithinCooldown_BroadcastsBoth()
    {
        _sessionVerifier.Add(Token, _player);
        _sessionVerifier.Add(OtherToken, _otherPlayer);
        _lobbyChatService.SendMessage(Token, CleanText, _now);

        _lobbyChatService.SendMessage(OtherToken, CleanText, _withinCooldown);

        Assert.Equal(TwoBroadcastLines, _broadcaster.Lines.Count);
    }

    [Fact]
    public void OpenLobbyChat_InvalidSession_ThrowsInvalidSession()
    {
        Action open = () => _lobbyChatService.OpenLobbyChat(UnknownToken, _now);

        Assert.Throws<InvalidSessionException>(open);
    }

    [Fact]
    public void OpenLobbyChat_AfterMessages_ReturnsThemInOrder()
    {
        _sessionVerifier.Add(Token, _player);
        _sessionVerifier.Add(OtherToken, _otherPlayer);
        _lobbyChatService.SendMessage(Token, FirstText, _now);
        _lobbyChatService.SendMessage(OtherToken, SecondText, _now);
        string[] expectedTexts = { FirstText, SecondText };

        LobbyChatHistory history = _lobbyChatService.OpenLobbyChat(Token, _now);

        IEnumerable<string> texts = history.Entries.Select(entry => entry.PostedLine.Line.Text);
        Assert.Equal(expectedTexts, texts);
    }

    [Fact]
    public void OpenLobbyChat_OwnMessage_MarksItAsOwn()
    {
        _sessionVerifier.Add(Token, _player);
        _lobbyChatService.SendMessage(Token, CleanText, _now);

        LobbyChatHistory history = _lobbyChatService.OpenLobbyChat(Token, _now);

        ChatHistoryEntry entry = history.Entries[0];
        Assert.True(entry.IsOwn);
    }

    [Fact]
    public void OpenLobbyChat_OtherPlayerMessage_DoesNotMarkItAsOwn()
    {
        _sessionVerifier.Add(Token, _player);
        _sessionVerifier.Add(OtherToken, _otherPlayer);
        _lobbyChatService.SendMessage(OtherToken, CleanText, _now);

        LobbyChatHistory history = _lobbyChatService.OpenLobbyChat(Token, _now);

        ChatHistoryEntry entry = history.Entries[0];
        Assert.False(entry.IsOwn);
    }

    [Fact]
    public void OpenLobbyChat_ManyMessages_ReturnsOnlyRecentCount()
    {
        _sessionVerifier.Add(Token, _player);
        int messageCount = ChatPolicy.MaximumRecentMessages + 1;
        SendMessagesFromManyPlayers(messageCount);

        LobbyChatHistory history = _lobbyChatService.OpenLobbyChat(Token, _now);

        Assert.Equal(ChatPolicy.MaximumRecentMessages, history.Entries.Count);
    }

    [Fact]
    public void OpenLobbyChat_Always_ReturnsListenerCount()
    {
        _sessionVerifier.Add(Token, _player);
        _broadcaster.ListenerCount = ListenerCount;

        LobbyChatHistory history = _lobbyChatService.OpenLobbyChat(Token, _now);

        Assert.Equal(ListenerCount, history.ConnectedPlayerCount);
    }

    [Fact]
    public void OpenLobbyChat_NoSanction_ReturnsNotMuted()
    {
        _sessionVerifier.Add(Token, _player);

        LobbyChatHistory history = _lobbyChatService.OpenLobbyChat(Token, _now);

        Assert.False(history.Mute.IsMuted);
    }

    [Fact]
    public void OpenLobbyChat_PermanentChatSanction_ReturnsMuteWithoutEnd()
    {
        _sessionVerifier.Add(Token, _player);
        AddSanction(SanctionScope.Chat, endsAt: null);
        var expectedMute = new ChatMute(IsMuted: true, EndsAt: null);

        LobbyChatHistory history = _lobbyChatService.OpenLobbyChat(Token, _now);

        Assert.Equal(expectedMute, history.Mute);
    }

    private void AddSanction(SanctionScope scope, DateTime? endsAt)
    {
        var sanction = new Sanction(AccountId, scope, endsAt);
        _unitOfWork.SanctionRepository.AddSanction(sanction);
    }

    private void SendMessagesFromManyPlayers(int count)
    {
        for (int playerIndex = 0; playerIndex < count; playerIndex++)
        {
            string token = $"{ManyPlayersTokenPrefix}{playerIndex}";
            string nickname = $"{ManyPlayersNicknamePrefix}{playerIndex}";
            var player = new ConnectedPlayer(playerIndex, nickname);
            _sessionVerifier.Add(token, player);
            _lobbyChatService.SendMessage(token, CleanText, _now);
        }
    }
}
