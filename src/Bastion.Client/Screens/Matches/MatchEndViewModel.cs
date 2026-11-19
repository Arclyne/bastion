using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Matches;
using Bastion.Client.Networking.Matchmaking;
using Bastion.Client.Networking.Notifications;
using Bastion.Domain.Matches;
using Gum.Mvvm;

namespace Bastion.Client.Screens.Matches;

public sealed class MatchEndViewModel : ViewModel, IDisposable
{
    private const char ReasonSeparator = ' ';
    private const byte NoLeaverTurnOrder = 0;

    private readonly JoinedMatch _joinedMatch;
    private readonly MatchPlayViewModel _matchPlay;
    private readonly byte _ownTurnOrder;
    private List<MatchEndRow> _rows;
    private MatchConnectionViewModel? _connection;

    public MatchEndViewModel(JoinedMatch joinedMatch, MatchPlayViewModel matchPlayViewModel)
    {
        ArgumentNullException.ThrowIfNull(joinedMatch);
        ArgumentNullException.ThrowIfNull(matchPlayViewModel);

        _joinedMatch = joinedMatch;
        _matchPlay = matchPlayViewModel;
        _ownTurnOrder = joinedMatch.GetOwnTurnOrder();
        _rows = [];
        KindText = string.Empty;
        TitleText = string.Empty;
        ReasonText = string.Empty;
        EloText = string.Empty;
        EloChangeText = string.Empty;
        SummaryText = string.Empty;
        _matchPlay.MatchEndReceived += HandleMatchEndReceived;
        _matchPlay.OwnParticipationEnded += HandleOwnParticipationEnded;
    }

    public bool IsShown
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public string KindText
    {
        get => Get<string>();
        private set => Set(value);
    }

    public string TitleText
    {
        get => Get<string>();
        private set => Set(value);
    }

    public string ReasonText
    {
        get => Get<string>();
        private set => Set(value);
    }

    public bool HasElo
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public string EloText
    {
        get => Get<string>();
        private set => Set(value);
    }

    public string EloChangeText
    {
        get => Get<string>();
        private set => Set(value);
    }

    public bool IsEloGain
    {
        get => Get<bool>();
        private set => Set(value);
    }

    [DependsOn(nameof(HasElo))]
    [DependsOn(nameof(IsEloGain))]
    public bool HasEloGain => HasElo && IsEloGain;

    [DependsOn(nameof(HasElo))]
    [DependsOn(nameof(IsEloGain))]
    public bool HasEloLoss => HasElo && !IsEloGain;

    public bool HasPrivateNotice
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public string SummaryText
    {
        get => Get<string>();
        private set => Set(value);
    }

    public IReadOnlyList<MatchEndRow> Rows => _rows;

    public event EventHandler? RowsChanged;

    public event EventHandler? ReturnToMenuRequested;

    public event EventHandler? SearchAgainRequested;

    public void FollowConnection(MatchConnectionViewModel connection)
    {
        ArgumentNullException.ThrowIfNull(connection);

        _connection = connection;
    }

    public void RequestReturnToMenu()
    {
        ReturnToMenuRequested?.Invoke(this, EventArgs.Empty);
    }

    public void RequestSearchAgain()
    {
        SearchAgainRequested?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        _matchPlay.MatchEndReceived -= HandleMatchEndReceived;
        _matchPlay.OwnParticipationEnded -= HandleOwnParticipationEnded;
    }

    private void HandleMatchEndReceived(object? sender, MatchEndedEventArgs e)
    {
        PlayerStanding ownStanding = FindOwnStanding(e.Standings);
        KindText = e.IsRanked ? Strings.MatchEndRankedKind : Strings.MatchEndPrivateKind;
        TitleText = MatchEndText.DescribeTitle(ownStanding, e.Standings.Count);
        ReasonText = DescribeReason(e.EndReason, e.DecidingTurnOrder);
        ShowElo(ownStanding, e.IsRanked);
        HasPrivateNotice = !e.IsRanked;
        SummaryText = MatchEndText.FormatSummary(e.MoveCount, e.PlayedSeconds, ownStanding.RemainingWalls);
        byte leaverTurnOrder = e.EndReason == EndedMatchReason.Abandonment ? e.DecidingTurnOrder : NoLeaverTurnOrder;
        ShowRows(e.Standings, leaverTurnOrder);
        IsShown = true;
    }

    private PlayerStanding FindOwnStanding(IReadOnlyList<PlayerStanding> standings)
    {
        return standings.First(standing => standing.TurnOrder == _ownTurnOrder);
    }

    private string DescribeReason(EndedMatchReason endReason, byte decidingTurnOrder)
    {
        if (endReason == EndedMatchReason.Abandonment && WasAwayAtEnd(decidingTurnOrder))
        {
            return DescribeLostConnection(decidingTurnOrder);
        }

        if (decidingTurnOrder == _ownTurnOrder)
        {
            return MatchEndText.DescribeOwnReason(endReason);
        }

        string decidingNickname = FindNickname(decidingTurnOrder);
        return MatchEndText.DescribeRivalReason(endReason, decidingNickname);
    }

    private bool WasAwayAtEnd(byte turnOrder)
    {
        return _connection is not null && _connection.WasAwayAtEnd(turnOrder);
    }

    private string DescribeLostConnection(byte decidingTurnOrder)
    {
        if (decidingTurnOrder == _ownTurnOrder)
        {
            return Strings.MatchEndOwnLostConnection;
        }

        string decidingNickname = FindNickname(decidingTurnOrder);
        return string.Format(CultureInfo.CurrentCulture, Strings.MatchEndRivalLostConnectionFormat, decidingNickname);
    }

    private string FindNickname(byte turnOrder)
    {
        FoundMatchParticipant? participant = _joinedMatch.FoundMatch.Participants.FirstOrDefault(
            candidate => candidate.TurnOrder == turnOrder);
        if (participant is null)
        {
            return string.Empty;
        }

        return participant.Nickname;
    }

    private void ShowElo(PlayerStanding ownStanding, bool isRanked)
    {
        HasElo = false;
        if (!isRanked)
        {
            return;
        }

        if (ownStanding.InitialElo is not short initialElo)
        {
            return;
        }

        if (ownStanding.FinalElo is not short finalElo)
        {
            return;
        }

        int eloChange = finalElo - initialElo;
        EloText = MatchEndText.FormatEloTransition(initialElo, finalElo);
        EloChangeText = MatchEndText.FormatEloChange(eloChange);
        IsEloGain = eloChange >= 0;
        HasElo = true;
    }

    private void ShowRows(IReadOnlyList<PlayerStanding> standings, byte leaverTurnOrder)
    {
        IEnumerable<MatchEndRow> rows = standings.Select(standing => ToRow(standing, standings.Count, leaverTurnOrder));
        _rows = [.. rows];
        RowsChanged?.Invoke(this, EventArgs.Empty);
    }

    private MatchEndRow ToRow(PlayerStanding standing, int playerCount, byte leaverTurnOrder)
    {
        bool hasAbandoned = standing.TurnOrder == leaverTurnOrder;
        string nickname = FindNickname(standing.TurnOrder);
        PawnSymbol pawnSymbol = Participation.GetPawnSymbol(standing.TurnOrder);
        string placeText = MatchEndText.FormatPlace(standing, playerCount);
        string resultText = MatchEndText.DescribeResult(standing, hasAbandoned);
        return new MatchEndRow
        {
            Nickname = nickname,
            IsOwn = standing.TurnOrder == _ownTurnOrder,
            PawnSymbol = pawnSymbol,
            PlaceText = placeText,
            ResultText = resultText,
        };
    }

    private void HandleOwnParticipationEnded(object? sender, ParticipationEndedEventArgs e)
    {
        PlayerStanding ownStanding = FindOwnStanding(e.Standings);
        string ownReason = MatchEndText.DescribeOwnReason(e.EndReason);
        string goesOn = MatchEndText.DescribeMatchGoesOn(e.ActivePlayerCount);
        KindText = Strings.MatchEndOwnPartKind;
        TitleText = MatchEndText.DescribeTitle(ownStanding, e.Standings.Count);
        ReasonText = string.Join(ReasonSeparator, ownReason, goesOn);
        HasElo = false;
        HasPrivateNotice = false;
        SummaryText = MatchEndText.FormatSummary(e.MoveCount, e.PlayedSeconds, ownStanding.RemainingWalls);
        ShowRows(e.Standings, NoLeaverTurnOrder);
        IsShown = true;
    }
}
