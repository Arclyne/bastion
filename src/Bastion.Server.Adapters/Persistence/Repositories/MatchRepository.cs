using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Chat;
using Bastion.Domain.Common;
using Bastion.Domain.GameModes;
using Bastion.Domain.Matches;
using Bastion.Domain.Rooms;
using Bastion.Server.Application.Matches;
using Microsoft.EntityFrameworkCore;

namespace Bastion.Server.Adapters.Persistence.Repositories;

public sealed class MatchRepository : IMatchRepository
{
    private readonly NewContextRunner<MatchRepositoryException> _newContextRunner;

    public MatchRepository(IDbContextFactory<BastionDbContext> contextFactory)
    {
        ArgumentNullException.ThrowIfNull(contextFactory);

        _newContextRunner = new NewContextRunner<MatchRepositoryException>(
            contextFactory,
            innerException => new MatchRepositoryException(innerException));
    }

    public bool HasUnfinishedParticipation(int accountId)
    {
        Func<BastionDbContext, bool> hasUnfinishedParticipation = context => HasUnfinishedParticipationInContext(
            context,
            accountId);
        return _newContextRunner.Run(hasUnfinishedParticipation);
    }

    public Lookup<GameMode> FindActiveGameMode(GameModeCode code)
    {
        Func<BastionDbContext, Lookup<GameMode>> findActiveGameMode = context => LoadActiveGameMode(context, code);
        return _newContextRunner.Run(findActiveGameMode);
    }

    public IReadOnlyDictionary<int, short> FindEloRatingsByAccountId(
        byte gameModeId,
        IReadOnlyCollection<int> accountIds)
    {
        ArgumentNullException.ThrowIfNull(accountIds);

        Func<BastionDbContext, Dictionary<int, short>> findEloRatings = context => LoadEloRatingsByAccountId(
            context,
            gameModeId,
            accountIds);
        return _newContextRunner.Run(findEloRatings);
    }

    public int SaveMatch(Match match)
    {
        ArgumentNullException.ThrowIfNull(match);

        Func<BastionDbContext, int> saveMatch = context => AddMatch(context, match);
        return _newContextRunner.Run(saveMatch);
    }

    public IReadOnlyList<int> FindAccountIdsWithUnfinishedParticipation(int matchId)
    {
        Func<BastionDbContext, List<int>> findAccountIds = context => LoadAccountIdsWithUnfinishedParticipation(
            context,
            matchId);
        return _newContextRunner.Run(findAccountIds);
    }

    public long SaveMatchChatMessage(Message message)
    {
        ArgumentNullException.ThrowIfNull(message);

        Func<BastionDbContext, long> saveMatchChatMessage = context => AddMatchChatMessage(context, message);
        return _newContextRunner.Run(saveMatchChatMessage);
    }

    public Lookup<Match> FindMatch(int matchId)
    {
        Func<BastionDbContext, Lookup<Match>> findMatch = context => LoadMatch(context, matchId);
        return _newContextRunner.Run(findMatch);
    }

    public void SaveAbandonment(Match match)
    {
        ArgumentNullException.ThrowIfNull(match);

        Func<BastionDbContext, int> saveAbandonment = context => UpdateMatchAndRoom(context, match);
        _newContextRunner.Run(saveAbandonment);
    }

    public Lookup<GameMode> FindGameMode(byte gameModeId)
    {
        Func<BastionDbContext, Lookup<GameMode>> findGameMode = context => LoadGameMode(context, gameModeId);
        return _newContextRunner.Run(findGameMode);
    }

    public void SaveMove(Match match)
    {
        ArgumentNullException.ThrowIfNull(match);

        Func<BastionDbContext, int> saveMove = context => UpdateMatchWithNewestMove(context, match);
        _newContextRunner.Run(saveMove);
    }

    private static bool HasUnfinishedParticipationInContext(BastionDbContext context, int accountId)
    {
        IQueryable<Participation> accountUnfinishedParticipations = SelectUnfinishedParticipations(context)
            .Where(participation => participation.AccountId == accountId);
        return accountUnfinishedParticipations.Any();
    }

    private static IQueryable<Participation> SelectUnfinishedParticipations(BastionDbContext context)
    {
        IQueryable<Participation> unfinishedParticipations = context.Participations
            .AsNoTracking()
            .Where(participation => !participation.Result.HasValue);
        return unfinishedParticipations;
    }

    private static Lookup<GameMode> LoadActiveGameMode(BastionDbContext context, GameModeCode code)
    {
        IQueryable<GameMode> activeModesWithCode = context.GameModes
            .AsNoTracking()
            .Where(gameMode => gameMode.Code == code)
            .Where(gameMode => gameMode.IsActive);
        GameMode? activeMode = activeModesWithCode.FirstOrDefault();
        return Lookup.FromNullable(activeMode, $"GameModeCode={code}");
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

    private static Lookup<Match> LoadMatch(BastionDbContext context, int matchId)
    {
        IQueryable<Match> matchesWithId = context.Matches
            .AsNoTracking()
            .Include(match => match.Participations)
            .Include(match => match.Moves)
            .Where(match => match.MatchId == matchId);
        Match? storedMatch = matchesWithId.FirstOrDefault();
        return Lookup.FromNullable(storedMatch, $"MatchId={matchId}");
    }

    private static int UpdateMatchAndRoom(BastionDbContext context, Match match)
    {
        context.Matches.Update(match);
        int savedRowCount = SaveMatchAndRoom(context, match);
        return savedRowCount;
    }

    private static int SaveMatchAndRoom(BastionDbContext context, Match match)
    {
        if (match.MatchType == MatchType.Private && match.IsFinished())
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
        if (room is null)
        {
            return;
        }

        DateTime endedAt = match.EndedAt ?? throw new InvalidOperationException("A finished match has no end time.");
        room.FinishMatch(endedAt);
    }

    private static Lookup<GameMode> LoadGameMode(BastionDbContext context, byte gameModeId)
    {
        IQueryable<GameMode> modesWithId = context.GameModes
            .AsNoTracking()
            .Where(gameMode => gameMode.GameModeId == gameModeId);
        GameMode? storedMode = modesWithId.FirstOrDefault();
        return Lookup.FromNullable(storedMode, $"GameModeId={gameModeId}");
    }

    private static int UpdateMatchWithNewestMove(BastionDbContext context, Match match)
    {
        context.Matches.Update(match);
        short newestMoveNumber = match.Moves.Max(move => move.MoveNumber);
        foreach (Move move in match.Moves)
        {
            bool isNewest = move.MoveNumber == newestMoveNumber;
            context.Entry(move).State = isNewest ? EntityState.Added : EntityState.Unchanged;
        }

        int savedRowCount = SaveMatchAndRoom(context, match);
        return savedRowCount;
    }
}
