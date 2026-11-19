using System;
using System.Linq;
using Bastion.Domain.GameModes;
using Bastion.Domain.Matches;
using Bastion.Server.Application.Matches;
using Bastion.Server.Application.Tests.Fakes;
using Bastion.Server.Application.Tests.GameModes;
using Xunit;

namespace Bastion.Server.Application.Tests.Matches;

public sealed class TestMatchRecoveryService
{
    private const int FirstAccountId = 12;
    private const int SecondAccountId = 13;
    private const short StartingElo = 1000;
    private const byte ClockMinutes = 5;

    private static readonly DateTime _startedAt = new DateTime(2026, 11, 19, 18, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime _restartedAt = new DateTime(2026, 11, 19, 18, 30, 0, DateTimeKind.Utc);

    private readonly FakeMatchRepository _matchRepository = new FakeMatchRepository();
    private readonly MatchRecoveryService _recoveryService;

    public TestMatchRecoveryService()
    {
        _recoveryService = new MatchRecoveryService(_matchRepository);
    }

    [Fact]
    public void RecoverMatchesAfterRestart_MatchInProgress_SavesIt()
    {
        AddMatchInProgress();

        _recoveryService.RecoverMatchesAfterRestart(_restartedAt);

        Assert.Single(_matchRepository.RecoveredMatches);
    }

    [Fact]
    public void RecoverMatchesAfterRestart_MatchInProgress_StartsTheTurnAtTheRestart()
    {
        AddMatchInProgress();

        _recoveryService.RecoverMatchesAfterRestart(_restartedAt);

        Match recoveredMatch = _matchRepository.RecoveredMatches.Single();
        Assert.Equal(_restartedAt, recoveredMatch.TurnStartedAt);
    }

    [Fact]
    public void RecoverMatchesAfterRestart_HalfAnHourDown_LeavesTheClockRunning()
    {
        AddMatchInProgress();

        _recoveryService.RecoverMatchesAfterRestart(_restartedAt);

        Match recoveredMatch = _matchRepository.RecoveredMatches.Single();
        Assert.False(recoveredMatch.HasClockRunOut(_restartedAt));
    }

    private void AddMatchInProgress()
    {
        GameMode classicMode = GameModeSampleFactory.CreateClassic();
        var format = new MatchFormat(classicMode, ClockMinutes);
        MatchEntrant[] entrants =
        [
            new MatchEntrant(FirstAccountId, StartingElo),
            new MatchEntrant(SecondAccountId, StartingElo),
        ];
        var match = Match.StartRanked(format, entrants, _startedAt);
        _matchRepository.MatchesInProgress.Add(match);
    }
}
