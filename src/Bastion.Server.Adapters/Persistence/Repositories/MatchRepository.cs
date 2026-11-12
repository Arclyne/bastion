using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Chat;
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
        return _newContextRunner.Run(context => HasUnfinishedParticipationInContext(context, accountId));
    }

    public GameMode? FindActiveGameMode(GameModeCode code)
    {
        return _newContextRunner.Run(context => LoadActiveGameMode(context, code));
    }

    public IReadOnlyDictionary<int, short> FindEloRatingsByAccountId(
        byte gameModeId,
        IReadOnlyCollection<int> accountIds)
    {
        ArgumentNullException.ThrowIfNull(accountIds);

        return _newContextRunner.Run(context => LoadEloRatingsByAccountId(context, gameModeId, accountIds));
    }

    public int SaveMatch(Match match)
    {
        ArgumentNullException.ThrowIfNull(match);

        return _newContextRunner.Run(context => AddMatch(context, match));
    }

    public IReadOnlyList<int> FindAccountIdsWithUnfinishedParticipation(int matchId)
    {
        return _newContextRunner.Run(context => LoadAccountIdsWithUnfinishedParticipation(context, matchId));
    }

    public long SaveMatchChatMessage(Message message)
    {
        ArgumentNullException.ThrowIfNull(message);

        return _newContextRunner.Run(context => AddMatchChatMessage(context, message));
    }

    public Match? FindMatch(int matchId)
    {
        return _newContextRunner.Run(context => LoadMatch(context, matchId));
    }

    public void SaveAbandonment(Match match)
    {
        ArgumentNullException.ThrowIfNull(match);

        _newContextRunner.Run(context => UpdateMatchAndRoom(context, match));
    }

    public GameMode? FindGameMode(byte gameModeId)
    {
        return _newContextRunner.Run(context => LoadGameMode(context, gameModeId));
    }

    public void SaveMove(Match match)
    {
        ArgumentNullException.ThrowIfNull(match);

        _newContextRunner.Run(context => UpdateMatchWithNewestMove(context, match));
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
            .Include(match => match.Moves)
            .Where(match => match.MatchId == matchId);
        return matchesWithId.FirstOrDefault();
    }

    private static GameMode? LoadGameMode(BastionDbContext context, byte gameModeId)
    {
        IQueryable<GameMode> modesWithId = context.GameModes
            .AsNoTracking()
            .Where(gameMode => gameMode.GameModeId == gameModeId);
        return modesWithId.FirstOrDefault();
    }

    private static int UpdateMatchAndRoom(BastionDbContext context, Match match)
    {
        context.Matches.Update(match);
        int savedRowCount = SaveMatchAndRoom(context, match);
        return savedRowCount;
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

    private static int SaveMatchAndRoom(BastionDbContext context, Match match)
    {
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

}
