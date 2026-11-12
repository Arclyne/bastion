using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Notifications;
using Bastion.Domain.Matches;
using Gum.Mvvm;

namespace Bastion.Client.Screens.Matches;

public sealed class MatchViewModel : ViewModel, IDisposable
{
    private const int TwoPlayers = 2;

    private readonly MatchAbandonmentViewModel _abandonmentViewModel;
    private readonly string _ownNickname;
    private List<BoardPawn> _pawns = new List<BoardPawn>();

    public MatchViewModel(
        JoinedMatch joinedMatch,
        MatchAbandonmentViewModel abandonmentViewModel,
        MatchPlayViewModel play)
    {
        ArgumentNullException.ThrowIfNull(joinedMatch);
        ArgumentNullException.ThrowIfNull(abandonmentViewModel);
        ArgumentNullException.ThrowIfNull(play);

        _abandonmentViewModel = abandonmentViewModel;
        Play = play;
        FoundMatchFormat format = joinedMatch.FoundMatch.Format;
        IEnumerable<MatchPlayerViewModel> playerQuery = joinedMatch.FoundMatch.Participants
            .OrderBy(participant => participant.TurnOrder)
            .Select(participant => CreatePlayerViewModel(joinedMatch, participant, format));
        Players = new List<MatchPlayerViewModel>(playerQuery);
        BoardSize = format.BoardSize;
        MatchPlayerViewModel ownPlayer = Players.First(player => player.IsOwn);
        OwnTurnOrder = ownPlayer.TurnOrder;
        _ownNickname = ownPlayer.Nickname;
        TurnText = string.Empty;
        foreach (string nickname in abandonmentViewModel.LeftNicknames)
        {
            MarkPlayerLeft(nickname);
        }

        RefreshPlay();
        _abandonmentViewModel.PlayerLeft += HandlePlayerLeft;
        Play.StateChanged += HandlePlayStateChanged;
    }

    public IReadOnlyList<MatchPlayerViewModel> Players { get; }

    public IReadOnlyList<BoardPawn> Pawns => _pawns;

    public MatchPlayViewModel Play { get; }

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
        Play.StateChanged -= HandlePlayStateChanged;
    }

    private static MatchPlayerViewModel CreatePlayerViewModel(
        JoinedMatch joinedMatch,
        FoundMatchParticipant participant,
        FoundMatchFormat format)
    {
        return new MatchPlayerViewModel(participant, format, joinedMatch.IsOwnParticipant(participant));
    }

    private void MarkPlayerLeft(string nickname)
    {
        MatchPlayerViewModel? player = Players.FirstOrDefault(candidate => candidate.Nickname == nickname);
        player?.MarkLeft();
    }

    private void RefreshPlay()
    {
        RebuildPawns();
        ShowTurn();
    }

    private void RebuildPawns()
    {
        var pawns = new List<BoardPawn>(Players.Count);
        foreach (MatchPlayerViewModel player in Players)
        {
            if (!player.HasLeft && Play.Pawns.TryGetValue(player.TurnOrder, out BoardCoordinates? coordinates))
            {
                pawns.Add(new BoardPawn(coordinates, player.PawnSymbol));
            }
        }

        _pawns = pawns;
    }

    private void ShowTurn()
    {
        if (Play.IsMatchOver)
        {
            HideTurns();
            TurnText = DescribeMatchEnd();
            return;
        }

        foreach (MatchPlayerViewModel player in Players)
        {
            player.ShowTurn(Play.CurrentTurnOrder);
        }

        MatchPlayerViewModel turnPlayer = Players.First(player => player.TurnOrder == Play.CurrentTurnOrder);
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
        string? winnerNickname = Play.WinnerNickname;
        if (winnerNickname is null)
        {
            return Strings.MatchOverTitle;
        }

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

        if (playerCount == TwoPlayers)
        {
            return Strings.MatchRivalTurn;
        }

        return string.Format(CultureInfo.CurrentCulture, Strings.MatchPlayerTurnFormat, turnPlayer.Nickname);
    }

    private void HandlePlayerLeft(object? sender, PlayerLeftEventArgs e)
    {
        MarkPlayerLeft(e.Nickname);
        RebuildPawns();
    }

    private void HandlePlayStateChanged(object? sender, EventArgs e)
    {
        RefreshPlay();
    }
}
