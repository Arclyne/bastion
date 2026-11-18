using System;
using Bastion.Domain.Moderation;
using Bastion.Server.Application.Chat;
using Xunit;

namespace Bastion.Server.Application.Tests.Chat;

public sealed class TestChatMuteStatus
{
    private const int AccountId = 5;
    private const int EarlierEndDays = 2;
    private const int LatestEndDays = 5;

    private static readonly DateTime _now = new DateTime(2026, 11, 5, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _earlierEnd = _now.AddDays(EarlierEndDays);

    [Fact]
    public void FromSanctionsInEffect_NoSanctions_ReturnsNone()
    {
        Sanction[] noSanctions = Array.Empty<Sanction>();

        var muteStatus = ChatMuteStatus.FromSanctionsInEffect(noSanctions);

        Assert.Equal(ChatMuteStatus.None, muteStatus);
    }

    [Fact]
    public void FromSanctionsInEffect_TwoTemporarySanctions_EndsWithTheLatest()
    {
        DateTime latestEnd = _now.AddDays(LatestEndDays);
        var earlierSanction = new Sanction(AccountId, SanctionScope.Chat, _earlierEnd);
        var latestSanction = new Sanction(AccountId, SanctionScope.Chat, latestEnd);
        Sanction[] sanctions = { earlierSanction, latestSanction };

        var muteStatus = ChatMuteStatus.FromSanctionsInEffect(sanctions);

        Assert.Equal(latestEnd, muteStatus.EndsAt);
    }

    [Fact]
    public void FromSanctionsInEffect_TemporaryAndPermanent_ReturnsPermanent()
    {
        var temporarySanction = new Sanction(AccountId, SanctionScope.Chat, _earlierEnd);
        var permanentSanction = new Sanction(AccountId, SanctionScope.Chat, endsAt: null);
        Sanction[] sanctions = { temporarySanction, permanentSanction };
        var expectedMuteStatus = new ChatMuteStatus(IsMuted: true, EndsAt: null);

        var muteStatus = ChatMuteStatus.FromSanctionsInEffect(sanctions);

        Assert.Equal(expectedMuteStatus, muteStatus);
    }
}
