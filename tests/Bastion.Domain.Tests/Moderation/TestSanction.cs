using System;
using Bastion.Domain.Moderation;
using Xunit;

namespace Bastion.Domain.Tests.Moderation;

public sealed class TestSanction
{
    private const int AccountId = 5;
    private const int DaysFromNow = 1;

    private static readonly DateTime _now = new DateTime(2026, 11, 5, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void IsInEffectAt_EndsLater_ReturnsTrue()
    {
        DateTime tomorrow = _now.AddDays(DaysFromNow);
        var sanction = new Sanction(AccountId, SanctionScope.Chat, tomorrow);

        bool isInEffect = sanction.IsInEffectAt(_now);

        Assert.True(isInEffect);
    }

    [Fact]
    public void IsInEffectAt_AlreadyEnded_ReturnsFalse()
    {
        DateTime yesterday = _now.AddDays(-DaysFromNow);
        var sanction = new Sanction(AccountId, SanctionScope.Chat, yesterday);

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
        var sanction = new Sanction(AccountId, SanctionScope.Chat, endsAt: null);

        bool isInEffect = sanction.IsInEffectAt(_now);

        Assert.True(isInEffect);
    }

    [Fact]
    public void IsPermanent_WithoutEnd_ReturnsTrue()
    {
        var sanction = new Sanction(AccountId, SanctionScope.Chat, endsAt: null);

        bool isPermanent = sanction.IsPermanent();

        Assert.True(isPermanent);
    }
}
