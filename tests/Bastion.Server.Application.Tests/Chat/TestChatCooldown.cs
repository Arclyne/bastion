using System;
using Bastion.Domain.Chat;
using Bastion.Server.Application.Chat;
using Xunit;

namespace Bastion.Server.Application.Tests.Chat;

public sealed class TestChatCooldown
{
    private const int AccountId = 4;
    private const int OtherAccountId = 5;
    private const long OneTick = 1;

    private static readonly DateTime _now = new DateTime(2026, 11, 5, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _cooldownEnd = _now.AddSeconds(ChatPolicy.MessageCooldownSeconds);

    private readonly ChatCooldown _cooldown = new ChatCooldown();

    [Fact]
    public void TryStartCooldown_FirstMessage_ReturnsTrue()
    {
        bool isCooldownStarted = _cooldown.TryStartCooldown(AccountId, _now);

        Assert.True(isCooldownStarted);
    }

    [Fact]
    public void TryStartCooldown_JustBeforeCooldownEnds_ReturnsFalse()
    {
        _cooldown.TryStartCooldown(AccountId, _now);
        DateTime justBeforeCooldownEnd = _cooldownEnd.AddTicks(-OneTick);

        bool isCooldownStarted = _cooldown.TryStartCooldown(AccountId, justBeforeCooldownEnd);

        Assert.False(isCooldownStarted);
    }

    [Fact]
    public void TryStartCooldown_AtCooldownEnd_ReturnsTrue()
    {
        _cooldown.TryStartCooldown(AccountId, _now);

        bool isCooldownStarted = _cooldown.TryStartCooldown(AccountId, _cooldownEnd);

        Assert.True(isCooldownStarted);
    }

    [Fact]
    public void TryStartCooldown_OtherAccountWithinCooldown_ReturnsTrue()
    {
        _cooldown.TryStartCooldown(AccountId, _now);

        bool isCooldownStarted = _cooldown.TryStartCooldown(OtherAccountId, _now);

        Assert.True(isCooldownStarted);
    }
}
