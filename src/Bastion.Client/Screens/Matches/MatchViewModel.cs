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
    private MatchConnectionViewModel? _connection;
    private MatchDrawViewModel? _draw;
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
        MatchEnd = new MatchEndViewModel(joinedMatch, matchPlayViewModel);
        FoundMatchFormat format = joinedMatch.FoundMatch.Format;
        IEnumerable<MatchPlayerViewModel> orderedPlayers = joinedMatch.FoundMatch.Participants
            .OrderBy(participant => participant.TurnOrder)
            .Select(participant => CreatePlayerViewModel(joinedMatch, participant, format));
        Players = [.. orderedPlayers];
        BoardSize = joinedMatch.GetBoardSize();
        OwnTurnOrder = joinedMatch.GetOwnTurnOrder();
        _ownNickname = joinedMatch.Session.Nickname;
        TurnText = string.Empty;
        foreach (PlayerLeftEventArgs departure in abandonmentViewModel.Departures)
        {
            MarkPlayerLeft(departure);
        }

        RefreshPlay();
        _abandonmentViewModel.PlayerLeft += HandlePlayerLeft;
        MatchPlay.StateChanged += HandleMatchPlayStateChanged;
        MatchPlay.ClockTicked += HandleClockTicked;
    }

    public IReadOnlyList<MatchPlayerViewModel> Players { get; }

    public IReadOnlyList<BoardPawn> Pawns => _pawns;

    public MatchPlayViewModel MatchPlay { get; }

    public WallPlacementViewModel WallPlacement { get; }

    public MatchEndViewModel MatchEnd { get; }

    public byte BoardSize { get; }

    public byte OwnTurnOrder { get; }

    public string TurnText
    {
        get => Get<string>();
        private set => Set(value);
    }

    public void FollowConnection(MatchConnectionViewModel connection)
    {
        ArgumentNullException.ThrowIfNull(connection);

        _connection = connection;
        MatchEnd.FollowConnection(connection);
        _connection.AwayPlayersChanged += HandleAwayPlayersChanged;
        ShowConnections();
    }

    public void FollowDraw(MatchDrawViewModel draw)
    {
        ArgumentNullException.ThrowIfNull(draw);

        _draw = draw;
        _draw.OwnOfferChanged += HandleOwnOfferChanged;
        ShowOwnDrawOffer();
    }

    public void Dispose()
    {
        if (_draw is not null)
        {
            _draw.OwnOfferChanged -= HandleOwnOfferChanged;
        }

        if (_connection is not null)
        {
            _connection.AwayPlayersChanged -= HandleAwayPlayersChanged;
        }

        _abandonmentViewModel.PlayerLeft -= HandlePlayerLeft;
        MatchPlay.StateChanged -= HandleMatchPlayStateChanged;
        MatchPlay.ClockTicked -= HandleClockTicked;
        WallPlacement.Dispose();
        MatchEnd.Dispose();
    }

    private static MatchPlayerViewModel CreatePlayerViewModel(
        JoinedMatch joinedMatch,
        FoundMatchParticipant participant,
        FoundMatchFormat format)
    {
        bool isOwn = joinedMatch.IsOwnParticipant(participant);
        return new MatchPlayerViewModel(participant, format, isOwn);
    }

    private void MarkPlayerLeft(PlayerLeftEventArgs departure)
    {
        MatchPlayerViewModel? player = Players.FirstOrDefault(candidate => candidate.Nickname == departure.Nickname);
        player?.MarkLeft(departure.EndReason);
    }

    [MemberNotNull(nameof(_pawns))]
    private void RefreshPlay()
    {
        _pawns = RebuildPawns();
        ShowWalls();
        ShowTurn();
        ShowClocks();
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

    private void ShowClocks()
    {
        foreach (MatchPlayerViewModel player in Players)
        {
            TimeSpan remainingClock = MatchPlay.GetRemainingClock(player.TurnOrder);
            player.ShowClock(remainingClock);
        }
    }

    private void HandlePlayerLeft(object? sender, PlayerLeftEventArgs e)
    {
        MarkPlayerLeft(e);
        _pawns = RebuildPawns();
    }

    private void HandleMatchPlayStateChanged(object? sender, EventArgs e)
    {
        RefreshPlay();
    }

    private void HandleAwayPlayersChanged(object? sender, EventArgs e)
    {
        ShowConnections();
    }

    private void ShowConnections()
    {
        if (_connection is null)
        {
            return;
        }

        foreach (MatchPlayerViewModel player in Players)
        {
            bool isAway = _connection.IsAway(player.TurnOrder);
            TimeSpan timeAway = _connection.GetTimeAway(player.TurnOrder);
            player.ShowConnection(isAway, timeAway);
        }
    }

    private void HandleOwnOfferChanged(object? sender, EventArgs e)
    {
        ShowOwnDrawOffer();
    }

    private void ShowOwnDrawOffer()
    {
        if (_draw is null)
        {
            return;
        }

        MatchPlayerViewModel ownPlayer = Players.First(player => player.IsOwn);
        ownPlayer.ShowDrawOffer(_draw.IsOwnOfferPending);
    }

    private void HandleClockTicked(object? sender, EventArgs e)
    {
        ShowClocks();
    }
}
