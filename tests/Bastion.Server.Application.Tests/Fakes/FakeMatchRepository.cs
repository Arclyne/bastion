using System.Collections.Generic;
using System;
using Bastion.Domain.Chat;
using Bastion.Domain.Common;
using Bastion.Domain.GameModes;
using Bastion.Domain.Matches;
using Bastion.Server.Application.Matches;
using Bastion.Server.Application.Tests.GameModes;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeMatchRepository : IMatchRepository
{
    public FakeMatchRepository()
    {
        AccountIdsWithUnfinishedParticipation = new HashSet<int>();
        ActiveGameMode = GameModeSampleFactory.CreateClassic();
        EloRatingsByAccountId = new Dictionary<int, short>();
        SavedMatches = new List<Match>();
        UnfinishedParticipantAccountIdsByMatchId = new Dictionary<int, List<int>>();
        SavedMatchChatMessages = new List<Message>();
        MatchesById = new Dictionary<int, Match>();
        MatchesSavedWithAbandonment = new List<Match>();
        MatchesSavedWithMove = new List<Match>();
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

    public List<Match> MatchesSavedWithAbandonment { get; }

    public List<Match> MatchesSavedWithMove { get; }

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
        if (!UnfinishedParticipantAccountIdsByMatchId.TryGetValue(matchId, out List<int>? accountIds))
        {
            return Array.Empty<int>();
        }

        return accountIds;
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
        if (!MatchesById.TryGetValue(matchId, out Match? match))
        {
            return Lookup.Missing<Match>($"MatchId={matchId}");
        }

        return Lookup.Found(match);
    }

    public void SaveAbandonment(Match match)
    {
        MatchesSavedWithAbandonment.Add(match);
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
}
