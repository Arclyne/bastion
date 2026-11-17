using System;
using Bastion.Domain.Accounts;
using Bastion.Domain.Sessions;
using Bastion.Server.Application.Accounts;
using Bastion.Server.Application.Sessions;
using Bastion.Server.Application.Tests.Fakes;
using Xunit;

namespace Bastion.Server.Application.Tests.Sessions;

public sealed class TestSessionVerifier
{
    private const string Nickname = "marta_muros";
    private const string Email = "marta@example.com";
    private const string UnknownToken = "otro-token";
    private const int MinutesAfterIssue = 1;

    private static readonly DateTime _issuedAt = new DateTime(2026, 10, 15, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _beforeExpiry = _issuedAt.AddMinutes(MinutesAfterIssue);
    private static readonly ClientConnection _connection = new ClientConnection("10.0.0.7", "device-1", "0.1.0");

    private readonly FakeUnitOfWork _unitOfWork = new FakeUnitOfWork();
    private readonly FakeSessionTokenGenerator _tokenGenerator = new FakeSessionTokenGenerator();
    private readonly SessionVerifier _verifier;

    public TestSessionVerifier()
    {
        _verifier = new SessionVerifier(_unitOfWork, _tokenGenerator);
        AddSession();
    }

    [Fact]
    public void Verify_UnknownToken_ThrowsInvalidSessionException()
    {
        Action verify = () => _verifier.Verify(UnknownToken, _beforeExpiry);

        Assert.Throws<InvalidSessionException>(verify);
    }

    [Fact]
    public void Verify_ExpiredSession_ThrowsInvalidSessionException()
    {
        DateTime afterExpiry = _issuedAt.AddHours(Session.MaximumLifetimeHours);

        Action verify = () => _verifier.Verify(FakeSessionTokenGenerator.Token, afterExpiry);

        Assert.Throws<InvalidSessionException>(verify);
    }

    [Fact]
    public void Verify_ActiveSession_ReturnsNickname()
    {
        ConnectedPlayer player = _verifier.Verify(FakeSessionTokenGenerator.Token, _beforeExpiry);

        Assert.Equal(Nickname, player.Nickname);
    }

    [Fact]
    public void VerifyIdentity_ActiveSession_ReturnsNickname()
    {
        PlayerIdentity identity = _verifier.VerifyIdentity(FakeSessionTokenGenerator.Token, _beforeExpiry);

        Assert.Equal(Nickname, identity.Nickname);
    }

    private void AddSession()
    {
        var account = new Account(Nickname, Email);
        _unitOfWork.AccountRepository.AddAccount(account);
        byte[] tokenHash = _tokenGenerator.Hash(FakeSessionTokenGenerator.Token);
        var ticket = new SessionTicket(tokenHash, _issuedAt);
        var session = new Session(account.AccountId, ticket, _connection);
        _unitOfWork.SessionRepository.Add(session);
    }
}
