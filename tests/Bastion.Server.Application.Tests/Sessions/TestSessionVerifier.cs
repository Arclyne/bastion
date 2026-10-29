using System;
using Bastion.Domain.Accounts;
using Bastion.Domain.Sessions;
using Bastion.Server.Application.Sessions;
using Bastion.Server.Application.Tests.Fakes;
using Xunit;

namespace Bastion.Server.Application.Tests.Sessions;

public class TestSessionVerifier
{
    private const string Nickname = "marta_muros";
    private const string Email = "marta@example.com";

    private static readonly DateTime _issuedAt = new DateTime(2026, 10, 15, 12, 0, 0, DateTimeKind.Utc);
    private static readonly ClientConnection _connection = new ClientConnection("10.0.0.7", "device-1", "0.1.0");

    private readonly FakeUnitOfWork _unitOfWork = new FakeUnitOfWork();
    private readonly FakeSessionTokenGenerator _tokenGenerator = new FakeSessionTokenGenerator();
    private readonly SessionVerifier _verifier;

    public TestSessionVerifier()
    {
        _verifier = new SessionVerifier(_unitOfWork, _tokenGenerator);
    }

    [Fact]
    public void Verify_UnknownToken_ThrowsInvalidSession()
    {
        StoreSession();

        Assert.Throws<InvalidSessionException>(() => _verifier.Verify("otro-token", _issuedAt.AddMinutes(1)));
    }

    [Fact]
    public void Verify_ExpiredSession_ThrowsInvalidSession()
    {
        StoreSession();
        DateTime afterExpiry = _issuedAt.AddHours(Session.MaximumLifetimeHours);

        Assert.Throws<InvalidSessionException>(() => _verifier.Verify(FakeSessionTokenGenerator.Token, afterExpiry));
    }

    [Fact]
    public void Verify_ActiveSession_ReturnsNickname()
    {
        StoreSession();

        ConnectedPlayer player = _verifier.Verify(FakeSessionTokenGenerator.Token, _issuedAt.AddMinutes(1));

        Assert.Equal(Nickname, player.Nickname);
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
