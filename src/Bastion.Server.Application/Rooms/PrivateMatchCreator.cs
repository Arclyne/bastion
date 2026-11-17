using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Matches;
using Bastion.Domain.Statistics;
using Bastion.Server.Application.Matches;
using Bastion.Server.Application.Matchmaking;
using Bastion.Server.Application.Sessions;
using log4net;

namespace Bastion.Server.Application.Rooms;

public sealed class PrivateMatchCreator : IPrivateMatchCreator
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(PrivateMatchCreator));

    private readonly IMatchRepository _matchRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly ITurnOrderShuffler _turnOrderShuffler;

    public PrivateMatchCreator(IMatchRepository matchRepository, IRoomRepository roomRepository, ITurnOrderShuffler turnOrderShuffler)
    {
        ArgumentNullException.ThrowIfNull(matchRepository);
        ArgumentNullException.ThrowIfNull(roomRepository);
        ArgumentNullException.ThrowIfNull(turnOrderShuffler);

        _matchRepository = matchRepository;
        _roomRepository = roomRepository;
        _turnOrderShuffler = turnOrderShuffler;
    }

    public CreatedMatch CreateMatch(RoomSnapshot room, DateTime startedAt)
    {
        ArgumentNullException.ThrowIfNull(room);

        IReadOnlyList<RoomMemberSnapshot> membersInTurnOrder = _turnOrderShuffler.ShuffleTurnOrder(room.Members);
        var format = new MatchFormat(room.Settings.GameMode, room.Settings.ClockMinutes)
        {
            WallsPerPlayer = room.Settings.WallsPerPlayer,
        };
        IEnumerable<int> accountIdsInTurnOrder = membersInTurnOrder.Select(member => member.AccountId);
        var accountIds = new List<int>(accountIdsInTurnOrder);
        IReadOnlyDictionary<int, short> eloRatingsByAccountId = _matchRepository.FindEloRatingsByAccountId(
            format.GameMode.GameModeId,
            accountIds);
        IEnumerable<MatchEntrant> unratedEntrants = membersInTurnOrder
            .Select(member => new MatchEntrant(member.AccountId, EloRating: null));
        var entrants = new List<MatchEntrant>(unratedEntrants);
        var match = Match.StartPrivate(format, entrants, startedAt);
        int matchId = _roomRepository.SaveRoomMatch(room.Identity.RoomId, match);
        _logger.Info($"Private match created. MatchId={matchId}; RoomId={room.Identity.RoomId}");
        List<CreatedMatchParticipant> participants = ToParticipants(match, membersInTurnOrder, eloRatingsByAccountId);
        return new CreatedMatch(matchId, format, participants);
    }

    private static List<CreatedMatchParticipant> ToParticipants(
        Match match,
        IReadOnlyList<RoomMemberSnapshot> membersInTurnOrder,
        IReadOnlyDictionary<int, short> eloRatingsByAccountId)
    {
        var participants = new List<CreatedMatchParticipant>();
        foreach (Participation participation in match.Participations)
        {
            RoomMemberSnapshot member = membersInTurnOrder.First(
                candidate => candidate.AccountId == participation.AccountId);
            short eloRating = eloRatingsByAccountId.GetValueOrDefault(member.AccountId, ModeStatistic.DefaultEloRating);
            var player = new ConnectedPlayer(member.AccountId, member.Nickname);
            var participant = new CreatedMatchParticipant(player, eloRating, participation.TurnOrder);
            participants.Add(participant);
        }

        return participants;
    }
}
