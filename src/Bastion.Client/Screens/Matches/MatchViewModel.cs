using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Matchmaking;
using Bastion.Domain.Matches;
using Gum.Mvvm;

namespace Bastion.Client.Screens.Matches;

public sealed class MatchViewModel : ViewModel, IDisposable
{
    private readonly MatchAbandonmentViewModel _abandonmentViewModel;
    private readonly string _ownNickname;
    private List<BoardPawn> _pawns;

    public MatchViewModel(
        JoinedMatch joinedMatch,
        MatchAbandonmentViewModel abandonmentViewModel,
        MatchPlayViewModel matchPlayViewModel)
    {
        ArgumentNullException.ThrowIfNull(joinedMatch);
        ArgumentNullException.ThrowIfNull(abandonmentViewModel);
        ArgumentNullException.ThrowIfNull(matchPlayViewModel);

        _abandonmentViewModel = abandonmentViewModel;
        MatchPlay = matchPlayViewModel;
        WallPlacement = new WallPlacementViewModel(matchPlayViewModel);
        FoundMatchFormat format = joinedMatch.FoundMatch.Format;
        IEnumerable<MatchPlayerViewModel> orderedPlayers = joinedMatch.FoundMatch.Participants
            .OrderBy(participant => participant.TurnOrder)
            .Select(participant => CreatePlayerViewModel(joinedMatch, participant, format));
        Players = [.. orderedPlayers];
        BoardSize = joinedMatch.GetBoardSize();
        OwnTurnOrder = joinedMatch.GetOwnTurnOrder();
        _ownNickname = joinedMatch.Session.Nickname;
        TurnText = string.Empty;
        foreach (string nickname in abandonmentViewModel.LeftNicknames)
        {
            MarkPlayerLeft(nickname);
        }

        RefreshPlay();
        _abandonmentViewModel.PlayerLeft += HandlePlayerLeft;
        MatchPlay.StateChanged += HandleMatchPlayStateChanged;
    }

    public IReadOnlyList<MatchPlayerViewModel> Players { get; }

    public IReadOnlyList<BoardPawn> Pawns => _pawns;

    public MatchPlayViewModel MatchPlay { get; }

    public WallPlacementViewModel WallPlacement { get; }

    public byte BoardSize { get; }

    public byte OwnTurnOrder { get; }

    public string TurnText
    {
        get => Get<string>();
        private set => Set(value);
    }

    public void Dispose()
    {
        _abandonmentViewModel.PlayerLeft -= HandlePlayerLeft;
        MatchPlay.StateChanged -= HandleMatchPlayStateChanged;
        WallPlacement.Dispose();
    }

    private static MatchPlayerViewModel CreatePlayerViewModel(
        JoinedMatch joinedMatch,
        FoundMatchParticipant participant,
        FoundMatchFormat format)
    {
        bool isOwn = joinedMatch.IsOwnParticipant(participant);
        return new MatchPlayerViewModel(participant, format, isOwn);
    }

    private void MarkPlayerLeft(string nickname)
    {
        MatchPlayerViewModel? player = Players.FirstOrDefault(candidate => candidate.Nickname == nickname);
        player?.MarkLeft();
    }

    [MemberNotNull(nameof(_pawns))]
    private void RefreshPlay()
    {
        _pawns = RebuildPawns();
        ShowWalls();
        ShowTurn();
    }

    private List<BoardPawn> RebuildPawns()
    {
        var pawns = new List<BoardPawn>(Players.Count);
        foreach (MatchPlayerViewModel player in Players)
        {
            BoardCoordinates? pawnCoordinates = MatchPlay.Pawns.GetValueOrDefault(player.TurnOrder);
            if (!player.HasLeft && pawnCoordinates is not null)
            {
                var pawn = new BoardPawn(pawnCoordinates, player.PawnSymbol);
                pawns.Add(pawn);
            }
        }

        return pawns;
    }

    private void ShowWalls()
    {
        foreach (MatchPlayerViewModel player in Players)
        {
            byte remainingWalls = MatchPlay.GetRemainingWalls(player.TurnOrder);
            player.ShowWalls(remainingWalls);
        }
    }

    private void ShowTurn()
    {
        if (MatchPlay.IsMatchOver)
        {
            HideTurns();
            TurnText = DescribeMatchEnd();
            return;
        }

        foreach (MatchPlayerViewModel player in Players)
        {
            player.ShowTurn(MatchPlay.CurrentTurnOrder);
        }

        MatchPlayerViewModel turnPlayer = Players.First(player => player.TurnOrder == MatchPlay.CurrentTurnOrder);
        TurnText = FormatTurn(turnPlayer, Players.Count);
    }

    private void HideTurns()
    {
        foreach (MatchPlayerViewModel player in Players)
        {
            player.HideTurn();
        }
    }

    private string DescribeMatchEnd()
    {
        if (!MatchPlay.HasWinner)
        {
            return Strings.MatchOverTitle;
        }

        string winnerNickname = MatchPlay.WinnerNickname;

        if (string.Equals(winnerNickname, _ownNickname, StringComparison.Ordinal))
        {
            return Strings.MatchYouWon;
        }

        return string.Format(CultureInfo.CurrentCulture, Strings.MatchPlayerWonFormat, winnerNickname);
    }

    private static string FormatTurn(MatchPlayerViewModel turnPlayer, int playerCount)
    {
        if (turnPlayer.IsOwn)
        {
            return Strings.MatchYourTurn;
        }

        if (playerCount == Match.TwoPlayerCount)
        {
            return Strings.MatchRivalTurn;
        }

        return string.Format(CultureInfo.CurrentCulture, Strings.MatchPlayerTurnFormat, turnPlayer.Nickname);
    }

    private void HandlePlayerLeft(object? sender, PlayerLeftEventArgs e)
    {
        MarkPlayerLeft(e.Nickname);
        _pawns = RebuildPawns();
    }

    private void HandleMatchPlayStateChanged(object? sender, EventArgs e)
    {
        RefreshPlay();
    }
}
