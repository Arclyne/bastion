using System;
using System.Collections.Generic;
using Bastion.Domain.Chat;
using Bastion.Domain.Common;
using Bastion.Domain.GameModes;
using Bastion.Domain.Matches;
using Bastion.Domain.Statistics;
using Bastion.Server.Application.Matches;
using Bastion.Server.Application.Tests.GameModes;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeMatchRepository : IMatchRepository
{
    public FakeMatchRepository()
    {
        AccountIdsWithUnfinishedParticipation = [];
        ActiveGameMode = GameModeSampleFactory.CreateClassic();
        EloRatingsByAccountId = [];
        SavedMatches = [];
        UnfinishedParticipantAccountIdsByMatchId = [];
        SavedMatchChatMessages = [];
        MatchesById = [];
        MatchesSavedWithDeparture = [];
        MatchesSavedWithMove = [];
        StoredModeStatistics = [];
        ClosuresSavedWithMove = [];
        ClosuresSavedWithoutMove = [];
        MatchIdsWithRunOutClock = [];
        MatchIdsWithAbsentPlayer = [];
        UnfinishedMatchIdsByAccountId = [];
        NicknamesByAccountId = [];
        MatchesSavedWithConnection = [];
        MatchesInProgress = [];
        RecoveredMatches = [];
        MatchesSavedWithDrawOffer = [];
    }

    public HashSet<int> AccountIdsWithUnfinishedParticipation { get; }

    public GameMode? ActiveGameMode { get; set; }

    public Dictionary<int, short> EloRatingsByAccountId { get; }

    public List<Match> SavedMatches { get; }

    public int NextMatchId { get; set; }

    public MatchRepositoryException? MatchSaveFailure { get; set; }

    public Dictionary<int, List<int>> UnfinishedParticipantAccountIdsByMatchId { get; }

    public List<Message> SavedMatchChatMessages { get; }

    public MatchRepositoryException? MatchChatMessageSaveFailure { get; set; }

    public Dictionary<int, Match> MatchesById { get; }

    public List<Match> MatchesSavedWithDeparture { get; }

    public List<Match> MatchesSavedWithMove { get; }

    public List<ModeStatistic> StoredModeStatistics { get; }

    public List<MatchClosure> ClosuresSavedWithMove { get; }

    public List<MatchClosure> ClosuresSavedWithoutMove { get; }

    public MatchRepositoryException? ClosureSaveFailure { get; set; }

    public List<int> MatchIdsWithRunOutClock { get; }

    public List<int> MatchIdsWithAbsentPlayer { get; }

    public MatchRepositoryException? MatchReadFailure { get; set; }

    public Dictionary<int, int> UnfinishedMatchIdsByAccountId { get; }

    public Dictionary<int, string> NicknamesByAccountId { get; }

    public List<Match> MatchesSavedWithConnection { get; }

    public List<Match> MatchesInProgress { get; }

    public List<Match> RecoveredMatches { get; }

    public List<Match> MatchesSavedWithDrawOffer { get; }

    public bool HasUnfinishedParticipation(int accountId)
    {
        return AccountIdsWithUnfinishedParticipation.Contains(accountId);
    }

    public Lookup<GameMode> FindActiveGameMode(GameModeCode code)
    {
        if (ActiveGameMode?.Code != code)
        {
            return Lookup.Missing<GameMode>($"GameModeCode={code}");
        }

        return Lookup.FromNullable(ActiveGameMode, $"GameModeCode={code}");
    }

    public IReadOnlyDictionary<int, short> FindEloRatingsByAccountId(
        byte gameModeId,
        IReadOnlyCollection<int> accountIds)
    {
        return EloRatingsByAccountId;
    }

    public int SaveMatch(Match match)
    {
        if (MatchSaveFailure is not null)
        {
            throw MatchSaveFailure;
        }

        SavedMatches.Add(match);
        return NextMatchId;
    }

    public IReadOnlyList<int> FindAccountIdsWithUnfinishedParticipation(int matchId)
    {
        IReadOnlyList<int>? accountIds = UnfinishedParticipantAccountIdsByMatchId.GetValueOrDefault(matchId);
        return accountIds ?? Array.Empty<int>();
    }

    public long SaveMatchChatMessage(Message message)
    {
        if (MatchChatMessageSaveFailure is not null)
        {
            throw MatchChatMessageSaveFailure;
        }

        SavedMatchChatMessages.Add(message);
        return SavedMatchChatMessages.Count;
    }

    public Lookup<Match> FindMatch(int matchId)
    {
        if (MatchReadFailure is not null)
        {
            throw MatchReadFailure;
        }

        Match? match = MatchesById.GetValueOrDefault(matchId);
        return Lookup.FromNullable(match, $"MatchId={matchId}");
    }

    public void SaveDeparture(Match match)
    {
        MatchesSavedWithDeparture.Add(match);
    }

    public Lookup<GameMode> FindGameMode(byte gameModeId)
    {
        if (ActiveGameMode?.GameModeId != gameModeId)
        {
            return Lookup.Missing<GameMode>($"GameModeId={gameModeId}");
        }

        return Lookup.FromNullable(ActiveGameMode, $"GameModeId={gameModeId}");
    }

    public void SaveMove(Match match)
    {
        MatchesSavedWithMove.Add(match);
    }

    public IReadOnlyList<ModeStatistic> FindModeStatistics(byte gameModeId, IReadOnlyCollection<int> accountIds)
    {
        return StoredModeStatistics;
    }

    public void SaveFinalMove(MatchClosure closure)
    {
        if (ClosureSaveFailure is not null)
        {
            throw ClosureSaveFailure;
        }

        ClosuresSavedWithMove.Add(closure);
    }

    public void SaveClosureWithoutMove(MatchClosure closure)
    {
        if (ClosureSaveFailure is not null)
        {
            throw ClosureSaveFailure;
        }

        ClosuresSavedWithoutMove.Add(closure);
    }

    public IReadOnlyList<int> FindMatchIdsWithRunOutClock(DateTime now)
    {
        return MatchIdsWithRunOutClock;
    }

    public IReadOnlyList<int> FindMatchIdsWithAbsentPlayer(DateTime now)
    {
        return MatchIdsWithAbsentPlayer;
    }

    public Lookup<int> FindUnfinishedMatchId(int accountId)
    {
        if (!UnfinishedMatchIdsByAccountId.TryGetValue(accountId, out int matchId))
        {
            return Lookup.Missing<int>($"AccountId={accountId}");
        }

        return Lookup.Found(matchId);
    }

    public IReadOnlyDictionary<int, string> FindNicknames(IReadOnlyCollection<int> accountIds)
    {
        return NicknamesByAccountId;
    }

    public void SaveConnection(Match match)
    {
        MatchesSavedWithConnection.Add(match);
    }

    public void SaveDrawOffer(Match match)
    {
        MatchesSavedWithDrawOffer.Add(match);
    }

    public IReadOnlyList<Match> FindMatchesInProgress()
    {
        return MatchesInProgress;
    }

    public void SaveRecoveredMatches(IReadOnlyList<Match> matches)
    {
        RecoveredMatches.AddRange(matches);
    }
}
