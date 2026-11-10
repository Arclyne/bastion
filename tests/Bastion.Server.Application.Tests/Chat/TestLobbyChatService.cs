using System;
using System.Linq;
using Bastion.Domain.Chat;
using Bastion.Domain.Moderation;
using Bastion.Server.Application.Chat;
using Bastion.Server.Application.Sessions;
using Bastion.Server.Application.Tests.Fakes;
using Xunit;

namespace Bastion.Server.Application.Tests.Chat;

public class TestLobbyChatService
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

    private static readonly DateTime _now = new DateTime(2026, 11, 5, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeUnitOfWork _unitOfWork = new FakeUnitOfWork();
    private readonly FakeSessionVerifier _sessionVerifier = new FakeSessionVerifier();
    private readonly FakeChatBroadcaster _broadcaster = new FakeChatBroadcaster();
    private readonly LobbyChatService _lobbyChatService;

    public TestLobbyChatService()
    {
        _lobbyChatService = new LobbyChatService(_sessionVerifier, _unitOfWork, new LobbyChatRoom(_broadcaster));
        _sessionVerifier.Add(Token, new ConnectedPlayer(AccountId, Nickname));
        _sessionVerifier.Add(OtherToken, new ConnectedPlayer(OtherAccountId, OtherNickname));
        _unitOfWork.BannedWordStore.ChatTerms.Add(BannedTerm);
        _broadcaster.ListenerCount = ListenerCount;
    }

    [Fact]
    public void SendMessage_InvalidSession_ThrowsInvalidSession()
    {
        Assert.Throws<InvalidSessionException>(() => _lobbyChatService.SendMessage(UnknownToken, "hola", _now));
    }

    [Fact]
    public void SendMessage_EmptyText_RejectsInvalidMessage()
    {
        ChatMessageRejectedException? rejection = RecordRejection("   ", _now);

        Assert.Equal(ChatRejection.InvalidMessage, rejection?.Rejection);
    }

    [Fact]
    public void SendMessage_TooLongText_RejectsInvalidMessage()
    {
        ChatMessageRejectedException? rejection = RecordRejection(
            new string('a', ChatPolicy.MaximumMessageLength + 1),
            _now);

        Assert.Equal(ChatRejection.InvalidMessage, rejection?.Rejection);
    }

    [Fact]
    public void SendMessage_ValidText_BroadcastsTrimmedText()
    {
        _lobbyChatService.SendMessage(Token, "  hola  ", _now);

        Assert.Equal("hola", _broadcaster.Lines[0].Line.Text);
    }

    [Fact]
    public void SendMessage_ValidText_BroadcastsSender()
    {
        _lobbyChatService.SendMessage(Token, "hola", _now);

        Assert.Equal(AccountId, _broadcaster.Lines[0].Line.Sender.AccountId);
    }

    [Fact]
    public void SendMessage_ValidText_ReturnsPostedLine()
    {
        PostedChatLine postedLine = _lobbyChatService.SendMessage(Token, "hola", _now);

        Assert.Equal(_broadcaster.Lines[0], postedLine);
    }

    [Fact]
    public void SendMessage_TemporaryChatSanction_RejectsMuted()
    {
        _unitOfWork.SanctionStore.Store(new Sanction(AccountId, SanctionScope.Chat, _now.AddDays(1)));

        ChatMessageRejectedException? rejection = RecordRejection("hola", _now);

        Assert.Equal(ChatRejection.Muted, rejection?.Rejection);
    }

    [Fact]
    public void SendMessage_TemporaryChatSanction_ReportsMuteEnd()
    {
        _unitOfWork.SanctionStore.Store(new Sanction(AccountId, SanctionScope.Chat, _now.AddDays(1)));

        ChatMessageRejectedException? rejection = RecordRejection("hola", _now);

        Assert.Equal(_now.AddDays(1), rejection?.MutedUntil);
    }

    [Fact]
    public void SendMessage_EndedChatSanction_BroadcastsMessage()
    {
        _unitOfWork.SanctionStore.Store(new Sanction(AccountId, SanctionScope.Chat, _now.AddDays(-1)));

        _lobbyChatService.SendMessage(Token, "hola", _now);

        Assert.Single(_broadcaster.Lines);
    }

    [Fact]
    public void SendMessage_AccountSanctionOnly_BroadcastsMessage()
    {
        _unitOfWork.SanctionStore.Store(new Sanction(AccountId, SanctionScope.Account, null));

        _lobbyChatService.SendMessage(Token, "hola", _now);

        Assert.Single(_broadcaster.Lines);
    }

    [Fact]
    public void SendMessage_BannedTerm_RejectsBannedTerm()
    {
        ChatMessageRejectedException? rejection = RecordRejection("eres un TONTO", _now);

        Assert.Equal(ChatRejection.BannedTerm, rejection?.Rejection);
    }

    [Fact]
    public void SendMessage_BannedTerm_DoesNotBroadcast()
    {
        Record.Exception(() => _lobbyChatService.SendMessage(Token, "tonto", _now));

        Assert.Empty(_broadcaster.Lines);
    }

    [Fact]
    public void SendMessage_SecondMessageWithinCooldown_RejectsTooFast()
    {
        _lobbyChatService.SendMessage(Token, "hola", _now);

        ChatMessageRejectedException? rejection = RecordRejection("otra vez", _now.AddSeconds(1));

        Assert.Equal(ChatRejection.TooFast, rejection?.Rejection);
    }

    [Fact]
    public void SendMessage_SecondMessageAfterCooldown_BroadcastsBoth()
    {
        _lobbyChatService.SendMessage(Token, "hola", _now);

        _lobbyChatService.SendMessage(Token, "otra vez", _now.AddSeconds(ChatPolicy.MessageCooldownSeconds));

        Assert.Equal(2, _broadcaster.Lines.Count);
    }

    [Fact]
    public void SendMessage_AfterRejectedMessage_KeepsTurn()
    {
        Record.Exception(() => _lobbyChatService.SendMessage(Token, "tonto", _now));

        _lobbyChatService.SendMessage(Token, "perdón", _now.AddSeconds(1));

        Assert.Single(_broadcaster.Lines);
    }

    [Fact]
    public void SendMessage_OtherPlayerWithinCooldown_BroadcastsBoth()
    {
        _lobbyChatService.SendMessage(Token, "hola", _now);

        _lobbyChatService.SendMessage(OtherToken, "hola", _now.AddSeconds(1));

        Assert.Equal(2, _broadcaster.Lines.Count);
    }

    [Fact]
    public void OpenLobbyChat_InvalidSession_ThrowsInvalidSession()
    {
        Assert.Throws<InvalidSessionException>(() => _lobbyChatService.OpenLobbyChat(UnknownToken, _now));
    }

    [Fact]
    public void OpenLobbyChat_AfterMessages_ReturnsThemInOrder()
    {
        _lobbyChatService.SendMessage(Token, "primero", _now);
        _lobbyChatService.SendMessage(OtherToken, "segundo", _now);

        LobbyChatHistory history = _lobbyChatService.OpenLobbyChat(Token, _now);

        Assert.Equal(new[] { "primero", "segundo" }, history.Entries.Select(entry => entry.Line.Line.Text));
    }

    [Fact]
    public void OpenLobbyChat_OwnMessage_MarksItAsOwn()
    {
        _lobbyChatService.SendMessage(Token, "hola", _now);

        LobbyChatHistory history = _lobbyChatService.OpenLobbyChat(Token, _now);

        Assert.True(history.Entries[0].IsOwn);
    }

    [Fact]
    public void OpenLobbyChat_OtherPlayerMessage_DoesNotMarkItAsOwn()
    {
        _lobbyChatService.SendMessage(OtherToken, "hola", _now);

        LobbyChatHistory history = _lobbyChatService.OpenLobbyChat(Token, _now);

        Assert.False(history.Entries[0].IsOwn);
    }

    [Fact]
    public void OpenLobbyChat_ManyMessages_ReturnsOnlyRecentCount()
    {
        SendMessagesFromManyPlayers(ChatPolicy.MaximumRecentMessages + 1);

        LobbyChatHistory history = _lobbyChatService.OpenLobbyChat(Token, _now);

        Assert.Equal(ChatPolicy.MaximumRecentMessages, history.Entries.Count);
    }

    [Fact]
    public void OpenLobbyChat_Always_ReturnsListenerCount()
    {
        LobbyChatHistory history = _lobbyChatService.OpenLobbyChat(Token, _now);

        Assert.Equal(ListenerCount, history.ConnectedPlayers);
    }

    [Fact]
    public void OpenLobbyChat_NoSanction_ReturnsNotMuted()
    {
        LobbyChatHistory history = _lobbyChatService.OpenLobbyChat(Token, _now);

        Assert.False(history.Mute.IsMuted);
    }

    [Fact]
    public void OpenLobbyChat_PermanentChatSanction_ReturnsMuteWithoutEnd()
    {
        _unitOfWork.SanctionStore.Store(new Sanction(AccountId, SanctionScope.Chat, null));

        LobbyChatHistory history = _lobbyChatService.OpenLobbyChat(Token, _now);

        Assert.Equal(new ChatMute(true, null), history.Mute);
    }

    private ChatMessageRejectedException? RecordRejection(string text, DateTime now)
    {
        return Record.Exception(() => _lobbyChatService.SendMessage(Token, text, now))
            as ChatMessageRejectedException;
    }

    private void SendMessagesFromManyPlayers(int count)
    {
        for (int playerIndex = 0; playerIndex < count; playerIndex++)
        {
            string token = $"player-{playerIndex}";
            _sessionVerifier.Add(token, new ConnectedPlayer(playerIndex, $"player_{playerIndex}"));
            _lobbyChatService.SendMessage(token, "hola", _now);
        }
    }
}
