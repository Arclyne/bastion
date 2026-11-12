using System;
using System.Collections.Generic;
using Bastion.Domain.Chat;
using Bastion.Domain.GameModes;
using Bastion.Domain.Matches;
using Bastion.Server.Application.Matches;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeMatchRepository : IMatchRepository
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

    public MatchRepositoryException? MatchSaveFailure { get; set; }

    public Dictionary<int, List<int>> UnfinishedParticipantAccountIdsByMatchId { get; } =
        new Dictionary<int, List<int>>();

    public List<Message> SavedMatchChatMessages { get; } = new List<Message>();

    public MatchRepositoryException? MatchChatMessageSaveFailure { get; set; }

    public Dictionary<int, Match> MatchesById { get; } = new Dictionary<int, Match>();

    public List<Match> SavedAbandonments { get; } = new List<Match>();

    public List<Match> SavedPawnMoves { get; } = new List<Match>();

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

        if (MatchSaveFailure is not null)
        {
            throw MatchSaveFailure;
        }

        SavedMatches.Add(match);
        return NextMatchId;
    }

    public IReadOnlyList<int> FindAccountIdsWithUnfinishedParticipation(int matchId)
    {
        return UnfinishedParticipantAccountIdsByMatchId.GetValueOrDefault(matchId) ?? new List<int>();
    }

    public long SaveMatchChatMessage(Message message)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (MatchChatMessageSaveFailure is not null)
        {
            throw MatchChatMessageSaveFailure;
        }

        SavedMatchChatMessages.Add(message);
        return SavedMatchChatMessages.Count;
    }

    public Match? FindMatch(int matchId)
    {
        return MatchesById.GetValueOrDefault(matchId);
    }

    public void SaveAbandonment(Match match)
    {
        ArgumentNullException.ThrowIfNull(match);

        SavedAbandonments.Add(match);
    }

    public GameMode? FindGameMode(byte gameModeId)
    {
        return ActiveGameMode?.GameModeId == gameModeId ? ActiveGameMode : null;
    }

    public void SavePawnMove(Match match)
    {
        ArgumentNullException.ThrowIfNull(match);

        SavedPawnMoves.Add(match);
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
