using System;
using System.Collections.Generic;
using Bastion.Domain.GameModes;
using Bastion.Domain.Matches;
using Bastion.Server.Application.Matches;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeMatchStore : IMatchStore
{
    private const byte ClassicGameModeId = 1;
    private const byte ClassicBoardSize = 9;
    private const byte ClassicPlayerCount = 2;
    private const byte ClassicWallsPerPlayer = 10;

    public HashSet<int> AccountIdsWithUnfinishedParticipation { get; } = new HashSet<int>();

    public GameMode? ActiveGameMode { get; set; } = CreateClassicGameMode();

    public Dictionary<int, short> EloRatingsByAccountId { get; } = new Dictionary<int, short>();

    public List<Match> SavedMatches { get; } = new List<Match>();

    public int NextMatchId { get; set; }

    public MatchStoreException? SaveFailure { get; set; }

    public bool HasUnfinishedParticipation(int accountId)
    {
        return AccountIdsWithUnfinishedParticipation.Contains(accountId);
    }

    public GameMode? FindActiveGameMode(GameModeCode code)
    {
        return ActiveGameMode?.Code == code ? ActiveGameMode : null;
    }

    public IReadOnlyDictionary<int, short> FindEloRatingsByAccountId(
        byte gameModeId,
        IReadOnlyCollection<int> accountIds)
    {
        ArgumentNullException.ThrowIfNull(accountIds);

        return EloRatingsByAccountId;
    }

    public int SaveMatch(Match match)
    {
        ArgumentNullException.ThrowIfNull(match);

        if (SaveFailure is not null)
        {
            throw SaveFailure;
        }

        SavedMatches.Add(match);
        return NextMatchId;
    }

    private static GameMode CreateClassicGameMode()
    {
        return new GameMode
        {
            GameModeId = ClassicGameModeId,
            Code = GameModeCode.Classic,
            BoardSize = ClassicBoardSize,
            PlayerCount = ClassicPlayerCount,
            WallsPerPlayer = ClassicWallsPerPlayer,
            IsActive = true,
        };
    }
}
