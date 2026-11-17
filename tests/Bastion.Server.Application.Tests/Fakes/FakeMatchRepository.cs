using System;
using System.Collections.Generic;
using Bastion.Domain.Chat;
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
        SavedMatchChatMessages = new List<Message>();
        MatchesById = new Dictionary<int, Match>();
        SavedAbandonments = new List<Match>();
        SavedMoves = new List<Match>();
    }

    public HashSet<int> AccountIdsWithUnfinishedParticipation { get; }

    public GameMode? ActiveGameMode { get; set; }

    public Dictionary<int, short> EloRatingsByAccountId { get; }

    public List<Match> SavedMatches { get; }

    public int NextMatchId { get; set; }

    public MatchRepositoryException? MatchSaveFailure { get; set; }

    public Dictionary<int, List<int>> UnfinishedParticipantAccountIdsByMatchId { get; } =
        new Dictionary<int, List<int>>();

    public List<Message> SavedMatchChatMessages { get; }

    public MatchRepositoryException? MatchChatMessageSaveFailure { get; set; }

    public Dictionary<int, Match> MatchesById { get; }

    public List<Match> SavedAbandonments { get; }

    public List<Match> SavedMoves { get; }

    public bool HasUnfinishedParticipation(int accountId)
    {
        return AccountIdsWithUnfinishedParticipation.Contains(accountId);
    }

    public GameMode? FindActiveGameMode(GameModeCode code)
    {
        if (ActiveGameMode?.Code != code)
        {
            return null;
        }

        return ActiveGameMode;
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

    public Match? FindMatch(int matchId)
    {
        return MatchesById.GetValueOrDefault(matchId);
    }

    public void SaveAbandonment(Match match)
    {
        SavedAbandonments.Add(match);
    }

    public GameMode? FindGameMode(byte gameModeId)
    {
        if (ActiveGameMode?.GameModeId != gameModeId)
        {
            return null;
        }

        return ActiveGameMode;
    }

    public void SaveMove(Match match)
    {
        SavedMoves.Add(match);
    }
}
