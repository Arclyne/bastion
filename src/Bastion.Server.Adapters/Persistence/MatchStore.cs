using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Bastion.Domain.Chat;
using Bastion.Domain.GameModes;
using Bastion.Domain.Matches;
using Bastion.Domain.Rooms;
using Bastion.Server.Application.Matches;
using Microsoft.EntityFrameworkCore;

namespace Bastion.Server.Adapters.Persistence;

public sealed class MatchStore : IMatchStore
{
    private readonly IDbContextFactory<BastionDbContext> _contextFactory;

    public MatchStore(IDbContextFactory<BastionDbContext> contextFactory)
    {
        ArgumentNullException.ThrowIfNull(contextFactory);

        _contextFactory = contextFactory;
    }

    public bool HasUnfinishedParticipation(int accountId)
    {
        return RunInNewContext(context => HasUnfinishedParticipationInContext(context, accountId));
    }

    public GameMode? FindActiveGameMode(GameModeCode code)
    {
        return RunInNewContext(context => LoadActiveGameMode(context, code));
    }

    public IReadOnlyDictionary<int, short> FindEloRatingsByAccountId(
        byte gameModeId,
        IReadOnlyCollection<int> accountIds)
    {
        ArgumentNullException.ThrowIfNull(accountIds);

        return RunInNewContext(context => LoadEloRatingsByAccountId(context, gameModeId, accountIds));
    }

    public int SaveMatch(Match match)
    {
        ArgumentNullException.ThrowIfNull(match);

        return RunInNewContext(context => AddMatch(context, match));
    }

    public IReadOnlyList<int> FindAccountIdsWithUnfinishedParticipation(int matchId)
    {
        return RunInNewContext(context => LoadAccountIdsWithUnfinishedParticipation(context, matchId));
    }

    public long SaveMatchChatMessage(Message message)
    {
        ArgumentNullException.ThrowIfNull(message);

        return RunInNewContext(context => AddMatchChatMessage(context, message));
    }

    public Match? FindMatch(int matchId)
    {
        return RunInNewContext(context => LoadMatch(context, matchId));
    }

    public void SaveAbandonment(Match match)
    {
        ArgumentNullException.ThrowIfNull(match);

        RunInNewContext(context => UpdateMatchAndRoom(context, match));
    }

    private static bool HasUnfinishedParticipationInContext(BastionDbContext context, int accountId)
    {
        IQueryable<Participation> accountUnfinishedParticipations = SelectUnfinishedParticipations(context)
            .Where(participation => participation.AccountId == accountId);
        return accountUnfinishedParticipations.Any();
    }

    private static GameMode? LoadActiveGameMode(BastionDbContext context, GameModeCode code)
    {
        IQueryable<GameMode> activeModesWithCode = context.GameModes
            .AsNoTracking()
            .Where(gameMode => gameMode.Code == code)
            .Where(gameMode => gameMode.IsActive);
        return activeModesWithCode.FirstOrDefault();
    }

    private static Dictionary<int, short> LoadEloRatingsByAccountId(
        BastionDbContext context,
        byte gameModeId,
        IReadOnlyCollection<int> accountIds)
    {
        IQueryable<KeyValuePair<int, short>> eloRatings = context.ModeStatistics
            .AsNoTracking()
            .Where(statistic => statistic.GameModeId == gameModeId)
            .Where(statistic => accountIds.Contains(statistic.AccountId))
            .Select(statistic => new KeyValuePair<int, short>(statistic.AccountId, statistic.EloRating));
        return new Dictionary<int, short>(eloRatings);
    }

    private static int AddMatch(BastionDbContext context, Match match)
    {
        context.Matches.Add(match);
        context.SaveChanges();
        return match.MatchId;
    }

    private static List<int> LoadAccountIdsWithUnfinishedParticipation(BastionDbContext context, int matchId)
    {
        IQueryable<int> accountIds = SelectUnfinishedParticipations(context)
            .Where(participation => participation.MatchId == matchId)
            .Select(participation => participation.AccountId);
        return new List<int>(accountIds);
    }

    private static long AddMatchChatMessage(BastionDbContext context, Message message)
    {
        context.Messages.Add(message);
        context.SaveChanges();
        return message.MessageId;
    }

    private static Match? LoadMatch(BastionDbContext context, int matchId)
    {
        IQueryable<Match> matchesWithId = context.Matches
            .AsNoTracking()
            .Include(match => match.Participations)
            .Where(match => match.MatchId == matchId);
        return matchesWithId.FirstOrDefault();
    }

    private static int UpdateMatchAndRoom(BastionDbContext context, Match match)
    {
        context.Matches.Update(match);
        if (match.MatchType == MatchType.Private && match.Status == MatchStatus.Finished)
        {
            FinishRoomOfMatch(context, match);
        }

        return context.SaveChanges();
    }

    private static void FinishRoomOfMatch(BastionDbContext context, Match match)
    {
        IQueryable<Room> roomsInMatch = context.Rooms
            .Where(candidate => candidate.MatchId == match.MatchId)
            .Where(candidate => candidate.Status == RoomStatus.InMatch);
        Room? room = roomsInMatch.FirstOrDefault();
        room?.FinishMatch(match.EndedAt.GetValueOrDefault(match.StartedAt));
    }

    private static IQueryable<Participation> SelectUnfinishedParticipations(BastionDbContext context)
    {
        IQueryable<Participation> unfinishedParticipations = context.Participations
            .AsNoTracking()
            .Where(participation => !participation.Result.HasValue);
        return unfinishedParticipations;
    }

    private TResult RunInNewContext<TResult>(Func<BastionDbContext, TResult> operation)
    {
        try
        {
            using BastionDbContext context = _contextFactory.CreateDbContext();
            return operation(context);
        }
        catch (DbUpdateException ex)
        {
            throw new MatchStoreException(ex);
        }
        catch (DbException ex)
        {
            throw new MatchStoreException(ex);
        }
    }
}
