using System;
using Bastion.Domain.Sessions;
using Xunit;

namespace Bastion.Domain.Tests.Sessions;

public class TestSession
{
    private static readonly DateTime _issuedAt = new DateTime(2026, 10, 15, 12, 0, 0, DateTimeKind.Utc);
    private static readonly ClientConnection _connection = new ClientConnection("10.0.0.7", "device-1", "0.1.0");

    [Fact]
    public void IsActiveAt_BeforeExpiry_ReturnsTrue()
    {
        Session session = CreateSession();

        bool isActive = session.IsActiveAt(_issuedAt.AddHours(1));

        Assert.True(isActive);
    }

    [Fact]
    public void IsActiveAt_AfterExpiry_ReturnsFalse()
    {
        Session session = CreateSession();

        bool isActive = session.IsActiveAt(_issuedAt.AddHours(Session.MaximumLifetimeHours));

        Assert.False(isActive);
    }

    private static Session CreateSession()
    {
        return new Session(1, new SessionTicket(new byte[] { 1, 2, 3 }, _issuedAt), _connection);
    }
}
