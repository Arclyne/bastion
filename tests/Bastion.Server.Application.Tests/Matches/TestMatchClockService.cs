using System;
using System.Linq;
using Bastion.Domain.GameModes;
using Bastion.Domain.Matches;
using Bastion.Server.Application.Matches;
using Bastion.Server.Application.Tests.Fakes;
using Bastion.Server.Application.Tests.GameModes;
using Xunit;

namespace Bastion.Server.Application.Tests.Matches;

public sealed class TestMatchClockService
{
    private const int MatchId = 31;
    private const int UnknownMatchId = 99;
    private const int FirstAccountId = 12;
    private const int SecondAccountId = 13;
    private const short StartingElo = 1000;
    private const byte ClockMinutes = 5;

    private static readonly DateTime _startedAt = new DateTime(2026, 11, 19, 18, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _beforeClockRanOut = new DateTime(2026, 11, 19, 18, 4, 0, DateTimeKind.Utc);
    private static readonly DateTime _afterClockRanOut = new DateTime(2026, 11, 19, 18, 6, 0, DateTimeKind.Utc);

    private readonly FakeMatchRepository _matchRepository = new FakeMatchRepository();
    private readonly FakeMatchEndNotifier _matchEndNotifier = new FakeMatchEndNotifier();
    private readonly MatchClockService _matchClockService;

    public TestMatchClockService()
    {
        var matchCloser = new MatchCloser(_matchRepository, _matchEndNotifier);
        _matchClockService = new MatchClockService(_matchRepository, matchCloser, new MatchGate());
    }

    [Fact]
    public void ExpireRunOutClocks_ClockRanOut_SavesClosure()
    {
        AddRankedMatch();

        _matchClockService.ExpireRunOutClocks(_afterClockRanOut);

        Assert.Single(_matchRepository.ClosuresSavedWithoutMove);
    }

    [Fact]
    public void ExpireRunOutClocks_ClockRanOut_AnnouncesTimeout()
    {
        AddRankedMatch();

        _matchClockService.ExpireRunOutClocks(_afterClockRanOut);

        NotifiedMatchEnd notified = _matchEndNotifier.NotifiedMatchEnds.Single();
        Assert.Equal(MatchEndReason.Timeout, notified.Notice.EndReason);
    }

    [Fact]
    public void ExpireRunOutClocks_ClockLeft_SavesNothing()
    {
        AddRankedMatch();

        _matchClockService.ExpireRunOutClocks(_beforeClockRanOut);

        Assert.Empty(_matchRepository.ClosuresSavedWithoutMove);
    }

    [Fact]
    public void ExpireRunOutClocks_MatchNoLongerStored_SavesNothing()
    {
        _matchRepository.MatchIdsWithRunOutClock.Add(UnknownMatchId);

        _matchClockService.ExpireRunOutClocks(_afterClockRanOut);

        Assert.Empty(_matchRepository.ClosuresSavedWithoutMove);
    }

    [Fact]
    public void ExpireRunOutClocks_MatchCannotBeRead_ThrowsNothing()
    {
        AddRankedMatch();
        _matchRepository.MatchReadFailure = new MatchRepositoryException("The match could not be read.");

        Action expireRunOutClocks = () => _matchClockService.ExpireRunOutClocks(_afterClockRanOut);

        Exception? failure = Record.Exception(expireRunOutClocks);
        Assert.Null(failure);
    }

    private void AddRankedMatch()
    {
        GameMode classicMode = GameModeSampleFactory.CreateClassic();
        var format = new MatchFormat(classicMode, ClockMinutes);
        MatchEntrant[] entrants =
        [
            new MatchEntrant(FirstAccountId, StartingElo),
            new MatchEntrant(SecondAccountId, StartingElo),
        ];
        var match = Match.StartRanked(format, entrants, _startedAt);
        _matchRepository.MatchesById[MatchId] = match;
        _matchRepository.MatchIdsWithRunOutClock.Add(MatchId);
    }
}
