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
    public HashSet<int> AccountIdsWithUnfinishedParticipation { get; } = new HashSet<int>();

    public GameMode? ActiveGameMode { get; set; } = GameModeSampleFactory.CreateClassic();

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
        if (!UnfinishedParticipantAccountIdsByMatchId.TryGetValue(matchId, out List<int>? accountIds))
        {
            return Array.Empty<int>();
        }

        return accountIds;
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
}
