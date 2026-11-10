using System;
using Bastion.Domain.Chat;
using Bastion.Server.Application.Chat;
using Xunit;

namespace Bastion.Server.Application.Tests.Chat;

public class TestChatCooldown
{
    private const int AccountId = 4;
    private const int OtherAccountId = 5;

    private static readonly DateTime _now = new DateTime(2026, 11, 5, 12, 0, 0, DateTimeKind.Utc);

    private readonly ChatCooldown _cooldown = new ChatCooldown();

    [Fact]
    public void TryStartCooldown_AtCooldownEnd_ReturnsTrue()
    {
        _cooldown.TryStartCooldown(AccountId, _now);

        bool claimed = _cooldown.TryStartCooldown(AccountId, _now.AddSeconds(ChatPolicy.MessageCooldownSeconds));

        Assert.True(claimed);
    }

    [Fact]
    public void TryStartCooldown_OtherAccountWithinCooldown_ReturnsTrue()
    {
        _cooldown.TryStartCooldown(AccountId, _now);

        bool claimed = _cooldown.TryStartCooldown(OtherAccountId, _now);

        Assert.True(claimed);
    }
}
