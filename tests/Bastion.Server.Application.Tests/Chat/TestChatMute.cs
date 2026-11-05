using System;
using Bastion.Domain.Moderation;
using Bastion.Server.Application.Chat;
using Xunit;

namespace Bastion.Server.Application.Tests.Chat;

public class TestChatMute
{
    private const int AccountId = 5;

    private static readonly DateTime _now = new DateTime(2026, 11, 5, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void FromSanctionsInEffect_NoSanctions_ReturnsNone()
    {
        var mute = ChatMute.FromSanctionsInEffect(Array.Empty<Sanction>());

        Assert.Equal(ChatMute.None, mute);
    }

    [Fact]
    public void FromSanctionsInEffect_TwoTemporarySanctions_EndsWithTheLatest()
    {
        Sanction[] sanctions =
        {
            new Sanction(AccountId, SanctionScope.Chat, _now.AddDays(2)),
            new Sanction(AccountId, SanctionScope.Chat, _now.AddDays(5)),
        };

        var mute = ChatMute.FromSanctionsInEffect(sanctions);

        Assert.Equal(_now.AddDays(5), mute.EndsAt);
    }

    [Fact]
    public void FromSanctionsInEffect_TemporaryAndPermanent_ReturnsPermanent()
    {
        Sanction[] sanctions =
        {
            new Sanction(AccountId, SanctionScope.Chat, _now.AddDays(2)),
            new Sanction(AccountId, SanctionScope.Chat, null),
        };

        var mute = ChatMute.FromSanctionsInEffect(sanctions);

        Assert.Equal(new ChatMute(true, null), mute);
    }
}
