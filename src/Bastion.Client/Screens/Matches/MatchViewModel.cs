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
    private readonly MatchAbandonmentViewModel _abandonmentViewModel;
    private List<BoardPawn> _pawns = new List<BoardPawn>();

    public MatchViewModel(JoinedMatch joinedMatch, MatchAbandonmentViewModel abandonmentViewModel)
    {
        ArgumentNullException.ThrowIfNull(joinedMatch);
        ArgumentNullException.ThrowIfNull(abandonmentViewModel);

        _abandonmentViewModel = abandonmentViewModel;
        FoundMatchFormat format = joinedMatch.FoundMatch.Format;
        IEnumerable<MatchPlayerViewModel> orderedPlayers = joinedMatch.FoundMatch.Participants
            .OrderBy(participant => participant.TurnOrder)
            .Select(participant => CreatePlayerViewModel(joinedMatch, participant, format));
        Players = new List<MatchPlayerViewModel>(orderedPlayers);
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
        bool isOwn = joinedMatch.IsOwnParticipant(participant);
        return new MatchPlayerViewModel(participant, format, isOwn);
    }

    private static string FormatTurn(MatchPlayerViewModel firstPlayer, int playerCount)
    {
        if (firstPlayer.IsOwn)
        {
            return Strings.MatchYourTurn;
        }

        if (playerCount == Match.TwoPlayerCount)
        {
            return Strings.MatchRivalTurn;
        }

        return string.Format(CultureInfo.CurrentCulture, Strings.MatchPlayerTurnFormat, firstPlayer.Nickname);
    }

    private void MarkPlayerLeft(string nickname)
    {
        MatchPlayerViewModel? player = Players.FirstOrDefault(candidate => candidate.Nickname == nickname);
        player?.MarkLeft();
    }

    private void RebuildPawns()
    {
        IEnumerable<BoardPawn> remainingPawns = Players
            .Where(player => !player.HasLeft)
            .Select(CreatePawn);
        _pawns = new List<BoardPawn>(remainingPawns);
    }

    private BoardPawn CreatePawn(MatchPlayerViewModel player)
    {
        BoardCoordinates coordinates = BoardLayout.GetStartingCoordinates(BoardSize, player.TurnOrder);
        return new BoardPawn(coordinates, player.PawnSymbol);
    }

    private void HandlePlayerLeft(object? sender, PlayerLeftEventArgs e)
    {
        MarkPlayerLeft(e.Nickname);
        RebuildPawns();
    }
}
