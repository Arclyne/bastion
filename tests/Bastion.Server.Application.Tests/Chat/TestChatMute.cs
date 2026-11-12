using System;
using Bastion.Domain.Moderation;
using Bastion.Server.Application.Chat;
using Xunit;

namespace Bastion.Server.Application.Tests.Chat;

public sealed class TestChatMute
{
    private const int AccountId = 5;
    private const int EarlierEndDays = 2;
    private const int LatestEndDays = 5;

    private static readonly DateTime _now = new DateTime(2026, 11, 5, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void FromSanctionsInEffect_NoSanctions_ReturnsNone()
    {
        Sanction[] noSanctions = Array.Empty<Sanction>();

        var mute = ChatMute.FromSanctionsInEffect(noSanctions);

        Assert.Equal(ChatMute.None, mute);
    }

    [Fact]
    public void FromSanctionsInEffect_TwoTemporarySanctions_EndsWithTheLatest()
    {
        DateTime earlierEnd = _now.AddDays(EarlierEndDays);
        DateTime latestEnd = _now.AddDays(LatestEndDays);
        var earlierSanction = new Sanction(AccountId, SanctionScope.Chat, earlierEnd);
        var latestSanction = new Sanction(AccountId, SanctionScope.Chat, latestEnd);
        Sanction[] sanctions = { earlierSanction, latestSanction };

        var mute = ChatMute.FromSanctionsInEffect(sanctions);

        Assert.Equal(latestEnd, mute.EndsAt);
    }

    [Fact]
    public void FromSanctionsInEffect_TemporaryAndPermanent_ReturnsPermanent()
    {
        DateTime temporaryEnd = _now.AddDays(EarlierEndDays);
        var temporarySanction = new Sanction(AccountId, SanctionScope.Chat, temporaryEnd);
        var permanentSanction = new Sanction(AccountId, SanctionScope.Chat, endsAt: null);
        Sanction[] sanctions = { temporarySanction, permanentSanction };
        var expectedMute = new ChatMute(IsMuted: true, EndsAt: null);

        var mute = ChatMute.FromSanctionsInEffect(sanctions);

        Assert.Equal(expectedMute, mute);
    }
}
