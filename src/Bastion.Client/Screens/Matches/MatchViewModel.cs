using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Notifications;
using Bastion.Domain.Matches;

namespace Bastion.Client.Screens.Matches;

public sealed class MatchViewModel : IDisposable
{
    private const int TwoPlayers = 2;

    private readonly MatchAbandonmentViewModel _abandonmentViewModel;
    private List<BoardPawn> _pawns = new List<BoardPawn>();

    public MatchViewModel(JoinedMatch joinedMatch, MatchAbandonmentViewModel abandonmentViewModel)
    {
        ArgumentNullException.ThrowIfNull(joinedMatch);
        ArgumentNullException.ThrowIfNull(abandonmentViewModel);

        _abandonmentViewModel = abandonmentViewModel;
        FoundMatchFormat format = joinedMatch.FoundMatch.Format;
        IEnumerable<MatchPlayerViewModel> playerQuery = joinedMatch.FoundMatch.Participants
            .OrderBy(participant => participant.TurnOrder)
            .Select(participant => CreatePlayerViewModel(joinedMatch, participant, format));
        Players = new List<MatchPlayerViewModel>(playerQuery);
        BoardSize = format.BoardSize;
        OwnTurnOrder = Players.First(player => player.IsOwn).TurnOrder;
        TurnText = FormatTurn(Players.First(player => player.HasFirstTurn()), Players.Count);
        foreach (string nickname in abandonmentViewModel.LeftNicknames)
        {
            MarkPlayerLeft(nickname);
        }

        RebuildPawns();
        _abandonmentViewModel.PlayerLeft += HandlePlayerLeft;
    }

    public IReadOnlyList<MatchPlayerViewModel> Players { get; }

    public IReadOnlyList<BoardPawn> Pawns => _pawns;

    public byte BoardSize { get; }

    public byte OwnTurnOrder { get; }

    public string TurnText { get; }

    public void Dispose()
    {
        _abandonmentViewModel.PlayerLeft -= HandlePlayerLeft;
    }

    private static MatchPlayerViewModel CreatePlayerViewModel(
        JoinedMatch joinedMatch,
        FoundMatchParticipant participant,
        FoundMatchFormat format)
    {
        return new MatchPlayerViewModel(participant, format, joinedMatch.IsOwnParticipant(participant));
    }

    private static string FormatTurn(MatchPlayerViewModel firstPlayer, int playerCount)
    {
        if (firstPlayer.IsOwn)
        {
            return Strings.MatchYourTurn;
        }

        if (playerCount == TwoPlayers)
        {
            return Strings.MatchRivalTurn;
        }

        return string.Format(CultureInfo.CurrentCulture, Strings.MatchPlayerTurnFormat, firstPlayer.Nickname);
    }

    private BoardPawn CreatePawn(MatchPlayerViewModel player)
    {
        BoardCoordinates coordinates = BoardLayout.GetStartingCoordinates(BoardSize, player.TurnOrder);
        return new BoardPawn(coordinates, player.PawnSymbol);
    }

    private void MarkPlayerLeft(string nickname)
    {
        MatchPlayerViewModel? player = Players.FirstOrDefault(candidate => candidate.Nickname == nickname);
        player?.MarkLeft();
    }

    private void RebuildPawns()
    {
        _pawns = Players.Where(player => !player.HasLeft).Select(CreatePawn).ToList();
    }

    private void HandlePlayerLeft(object? sender, PlayerLeftEventArgs e)
    {
        MarkPlayerLeft(e.Nickname);
        RebuildPawns();
    }
}
