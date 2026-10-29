using System;
using Bastion.Domain.Accounts;
using Bastion.Domain.Sessions;
using Bastion.Server.Application.Chat;
using Bastion.Server.Application.Sessions;
using Bastion.Server.Application.Tests.Fakes;
using Xunit;

namespace Bastion.Server.Application.Tests.Chat;

public class TestLobbyChatService
{
    private const string Nickname = "marta_muros";
    private const string Email = "marta@example.com";

    private static readonly DateTime _issuedAt = new DateTime(2026, 10, 15, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _now = _issuedAt.AddMinutes(1);
    private static readonly ClientConnection _connection = new ClientConnection("10.0.0.7", "device-1", "0.1.0");

    private readonly FakeUnitOfWork _unitOfWork = new FakeUnitOfWork();
    private readonly FakeSessionTokenGenerator _tokenGenerator = new FakeSessionTokenGenerator();
    private readonly FakeChatBroadcaster _broadcaster = new FakeChatBroadcaster();
    private readonly LobbyChatService _service;

    public TestLobbyChatService()
    {
        _service = new LobbyChatService(new SessionVerifier(_unitOfWork, _tokenGenerator), _broadcaster);
        StoreSession();
    }

    [Fact]
    public void Send_InvalidSession_ThrowsInvalidSession()
    {
        Assert.Throws<InvalidSessionException>(() => _service.Send("otro-token", "hola", _now));
    }

    [Fact]
    public void Send_EmptyText_ThrowsInvalidChatMessage()
    {
        Assert.Throws<InvalidChatMessageException>(() => _service.Send(FakeSessionTokenGenerator.Token, "   ", _now));
    }

    [Fact]
    public void Send_ValidText_BroadcastsTrimmedText()
    {
        _service.Send(FakeSessionTokenGenerator.Token, "  hola  ", _now);

        Assert.Equal("hola", _broadcaster.Lines[0].Text);
    }

    [Fact]
    public void Send_ValidText_BroadcastsSenderNickname()
    {
        _service.Send(FakeSessionTokenGenerator.Token, "hola", _now);

        Assert.Equal(Nickname, _broadcaster.Lines[0].SenderNickname);
    }

    private void StoreSession()
    {
        var account = new Account(Nickname, Email);
        _unitOfWork.AccountStore.Store(account);
        byte[] tokenHash = _tokenGenerator.Hash(FakeSessionTokenGenerator.Token);
        var ticket = new SessionTicket(tokenHash, _issuedAt);
        _unitOfWork.SessionStore.Add(new Session(account.AccountId, ticket, _connection));
    }
}
