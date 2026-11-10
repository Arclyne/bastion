using System;
using Bastion.Domain.Moderation;
using Xunit;

namespace Bastion.Domain.Tests.Moderation;

public class TestSanction
{
    private const int AccountId = 5;

    private static readonly DateTime _now = new DateTime(2026, 11, 5, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void IsInEffectAt_EndsLater_ReturnsTrue()
    {
        var sanction = new Sanction(AccountId, SanctionScope.Chat, _now.AddDays(1));

        bool isInEffect = sanction.IsInEffectAt(_now);

        Assert.True(isInEffect);
    }

    [Fact]
    public void IsInEffectAt_AlreadyEnded_ReturnsFalse()
    {
        var sanction = new Sanction(AccountId, SanctionScope.Chat, _now.AddDays(-1));

        bool isInEffect = sanction.IsInEffectAt(_now);

        Assert.False(isInEffect);
    }

    [Fact]
    public void IsInEffectAt_EndsNow_ReturnsFalse()
    {
        var sanction = new Sanction(AccountId, SanctionScope.Chat, _now);

        bool isInEffect = sanction.IsInEffectAt(_now);

        Assert.False(isInEffect);
    }

    [Fact]
    public void IsInEffectAt_Permanent_ReturnsTrue()
    {
        var sanction = new Sanction(AccountId, SanctionScope.Chat, null);

        bool isInEffect = sanction.IsInEffectAt(_now);

        Assert.True(isInEffect);
    }

    [Fact]
    public void IsPermanent_WithoutEnd_ReturnsTrue()
    {
        var sanction = new Sanction(AccountId, SanctionScope.Chat, null);

        bool isPermanent = sanction.IsPermanent();

        Assert.True(isPermanent);
    }
}
