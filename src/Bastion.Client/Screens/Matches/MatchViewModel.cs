using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Notifications;
using Bastion.Domain.Matches;

namespace Bastion.Client.Screens.Matches;

public sealed class MatchViewModel
{
    public MatchViewModel(JoinedMatch joinedMatch)
    {
        ArgumentNullException.ThrowIfNull(joinedMatch);

        FoundMatchFormat format = joinedMatch.FoundMatch.Format;
        IEnumerable<MatchPlayerViewModel> playerQuery = joinedMatch.FoundMatch.Participants
            .OrderBy(participant => participant.TurnOrder)
            .Select(participant => CreatePlayerViewModel(joinedMatch, participant, format));
        Players = new List<MatchPlayerViewModel>(playerQuery);

        IEnumerable<BoardPawn> pawnQuery = Players.Select(player => CreatePawn(format.BoardSize, player));
        Pawns = new List<BoardPawn>(pawnQuery);

        MatchPlayerViewModel own = Players.First(player => player.IsOwn);
        BoardSize = format.BoardSize;
        IsBoardRotated = !own.HasFirstTurn();
        TurnTextKey = own.HasFirstTurn() ? nameof(Strings.MatchYourTurn) : nameof(Strings.MatchRivalTurn);
    }

    public IReadOnlyList<MatchPlayerViewModel> Players { get; }

    public IReadOnlyList<BoardPawn> Pawns { get; }

    public byte BoardSize { get; }

    public bool IsBoardRotated { get; }

    public string TurnTextKey { get; }

    private static MatchPlayerViewModel CreatePlayerViewModel(
        JoinedMatch joinedMatch,
        FoundMatchParticipant participant,
        FoundMatchFormat format)
    {
        return new MatchPlayerViewModel(participant, format, joinedMatch.IsOwnParticipant(participant));
    }

    private static BoardPawn CreatePawn(byte boardSize, MatchPlayerViewModel player)
    {
        BoardCoordinates coordinates = BoardLayout.GetStartingCoordinates(boardSize, player.TurnOrder);
        return new BoardPawn(coordinates, player.PawnSymbol);
    }
}
