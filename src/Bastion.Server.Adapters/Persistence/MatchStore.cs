using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Bastion.Domain.Chat;
using Bastion.Domain.GameModes;
using Bastion.Domain.Matches;
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

    public void SaveMatchEnd(Match match)
    {
        ArgumentNullException.ThrowIfNull(match);

        RunInNewContext(context => UpdateMatch(context, match));
    }

    private static bool HasUnfinishedParticipationInContext(BastionDbContext context, int accountId)
    {
        return context.Participations.Any(
            participation => participation.AccountId == accountId && !participation.Result.HasValue);
    }

    private static GameMode? LoadActiveGameMode(BastionDbContext context, GameModeCode code)
    {
        return context.GameModes
            .AsNoTracking()
            .FirstOrDefault(gameMode => gameMode.Code == code && gameMode.IsActive);
    }

    private static Dictionary<int, short> LoadEloRatingsByAccountId(
        BastionDbContext context,
        byte gameModeId,
        IReadOnlyCollection<int> accountIds)
    {
        return context.ModeStatistics
            .AsNoTracking()
            .Where(statistic => statistic.GameModeId == gameModeId && accountIds.Contains(statistic.AccountId))
            .ToDictionary(statistic => statistic.AccountId, statistic => statistic.EloRating);
    }

    private static int AddMatch(BastionDbContext context, Match match)
    {
        // One SaveChanges writes the match and its participations in a single transaction (CU-09 RN-08).
        context.Matches.Add(match);
        context.SaveChanges();
        return match.MatchId;
    }

    private static List<int> LoadAccountIdsWithUnfinishedParticipation(BastionDbContext context, int matchId)
    {
        return context.Participations
            .AsNoTracking()
            .Where(participation => participation.MatchId == matchId && !participation.Result.HasValue)
            .Select(participation => participation.AccountId)
            .ToList();
    }

    private static long AddMatchChatMessage(BastionDbContext context, Message message)
    {
        context.Messages.Add(message);
        context.SaveChanges();
        return message.MessageId;
    }

    private static Match? LoadMatch(BastionDbContext context, int matchId)
    {
        return context.Matches
            .AsNoTracking()
            .Include(match => match.Participations)
            .FirstOrDefault(match => match.MatchId == matchId);
    }

    private static int UpdateMatch(BastionDbContext context, Match match)
    {
        // The match and its participations are written in one SaveChanges, so they cannot end apart (CU-18 RN-10).
        context.Matches.Update(match);
        return context.SaveChanges();
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
