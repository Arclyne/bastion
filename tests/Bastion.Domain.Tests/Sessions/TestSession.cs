using System;
using Bastion.Domain.Sessions;
using Xunit;

namespace Bastion.Domain.Tests.Sessions;

public sealed class TestSession
{
    private const int AccountId = 1;
    private const int HoursAfterIssue = 1;

    private static readonly DateTime _issuedAt = new DateTime(2026, 10, 15, 12, 0, 0, DateTimeKind.Utc);
    private static readonly ClientConnection _connection = new ClientConnection("10.0.0.7", "device-1", "0.1.0");
    private static readonly byte[] _tokenHash = { 1, 2, 3 };

    [Fact]
    public void IsActiveAt_BeforeExpiry_ReturnsTrue()
    {
        Session session = CreateSession();
        DateTime beforeExpiry = _issuedAt.AddHours(HoursAfterIssue);

        bool isActive = session.IsActiveAt(beforeExpiry);

        Assert.True(isActive);
    }

    [Fact]
    public void IsActiveAt_AfterExpiry_ReturnsFalse()
    {
        Session session = CreateSession();
        DateTime expiry = _issuedAt.AddHours(Session.MaximumLifetimeHours);

        bool isActive = session.IsActiveAt(expiry);

        Assert.False(isActive);
    }

    private static Session CreateSession()
    {
        var ticket = new SessionTicket(_tokenHash, _issuedAt);
        return new Session(AccountId, ticket, _connection);
    }
}
