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

    private readonly IMatchStore _matchStore;
    private readonly IRoomStore _roomStore;
    private readonly ITurnOrderShuffler _turnOrderShuffler;

    public PrivateMatchCreator(IMatchStore matchStore, IRoomStore roomStore, ITurnOrderShuffler turnOrderShuffler)
    {
        ArgumentNullException.ThrowIfNull(matchStore);
        ArgumentNullException.ThrowIfNull(roomStore);
        ArgumentNullException.ThrowIfNull(turnOrderShuffler);

        _matchStore = matchStore;
        _roomStore = roomStore;
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
        IReadOnlyDictionary<int, short> eloRatingsByAccountId =
            _matchStore.FindEloRatingsByAccountId(format.GameMode.GameModeId, accountIds);
        IEnumerable<MatchEntrant> entrants = membersInTurnOrder.Select(member => new MatchEntrant(member.AccountId, null));
        var entrantList = new List<MatchEntrant>(entrants);
        int matchId = _roomStore.SaveRoomMatch(room.Identity.RoomId, Match.StartPrivate(format, entrantList, startedAt));
        _logger.Info($"Private match created. MatchId={matchId}; RoomId={room.Identity.RoomId}");
        List<CreatedMatchParticipant> participants = ToParticipants(membersInTurnOrder, eloRatingsByAccountId);
        return new CreatedMatch(matchId, format, participants);
    }

    private static List<CreatedMatchParticipant> ToParticipants(
        IReadOnlyList<RoomMemberSnapshot> membersInTurnOrder,
        IReadOnlyDictionary<int, short> eloRatingsByAccountId)
    {
        var participants = new List<CreatedMatchParticipant>();
        for (int memberIndex = 0; memberIndex < membersInTurnOrder.Count; memberIndex++)
        {
            RoomMemberSnapshot member = membersInTurnOrder[memberIndex];
            short eloRating = eloRatingsByAccountId.GetValueOrDefault(member.AccountId, ModeStatistic.DefaultEloRating);
            var player = new ConnectedPlayer(member.AccountId, member.Nickname);
            int turnOrderNumber = Match.FirstTurnOrder + memberIndex;
            byte turnOrder = (byte)turnOrderNumber;
            participants.Add(new CreatedMatchParticipant(player, eloRating, turnOrder));
        }

        return participants;
    }
}
