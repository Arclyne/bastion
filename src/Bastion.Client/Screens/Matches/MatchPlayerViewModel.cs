using System;
using System.Globalization;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Matches;
using Bastion.Client.Networking.Matchmaking;
using Bastion.Domain.Matches;
using Gum.Mvvm;

namespace Bastion.Client.Screens.Matches;

public sealed class MatchPlayerViewModel : ViewModel
{
    public const int LowClockSeconds = 30;

    private static readonly TimeSpan _lowClockThreshold = TimeSpan.FromSeconds(LowClockSeconds);

    private readonly string _eloText;
    private string _departureText;

    public MatchPlayerViewModel(FoundMatchParticipant participant, FoundMatchFormat format, bool isOwn)
    {
        ArgumentNullException.ThrowIfNull(participant);
        ArgumentNullException.ThrowIfNull(format);

        Nickname = participant.Nickname;
        _eloText = MatchScreenText.FormatElo(participant.EloRating);
        _departureText = string.Empty;
        AwayText = string.Empty;
        WallsText = MatchScreenText.FormatWallCount(format.WallsPerPlayer);
        ClockText = MatchScreenText.FormatClock(format.ClockMinutes);
        PawnSymbol = Participation.GetPawnSymbol(participant.TurnOrder);
        TurnOrder = participant.TurnOrder;
        IsOwn = isOwn;
    }

    public string Nickname { get; }

    public string WallsText
    {
        get => Get<string>();
        private set => Set(value);
    }

    public string ClockText
    {
        get => Get<string>();
        private set => Set(value);
    }

    public bool IsClockLow
    {
        get => Get<bool>();
        private set => Set(value);
    }

    [DependsOn(nameof(HasTurn))]
    [DependsOn(nameof(IsClockLow))]
    public bool IsClockRunning => HasTurn && !IsClockLow;

    [DependsOn(nameof(HasTurn))]
    [DependsOn(nameof(IsClockLow))]
    public bool IsClockWaiting => !HasTurn && !IsClockLow;

    public PawnSymbol PawnSymbol { get; }

    public byte TurnOrder { get; }

    public bool IsOwn { get; }

    public bool HasLeft
    {
        get => Get<bool>();
        private set => Set(value);
    }

    [DependsOn(nameof(HasLeft))]
    public string DetailText => HasLeft ? _departureText : _eloText;

    public bool IsAway
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public string AwayText
    {
        get => Get<string>();
        private set => Set(value);
    }

    public bool HasOfferedDraw
    {
        get => Get<bool>();
        private set => Set(value);
    }

    [DependsOn(nameof(HasLeft))]
    [DependsOn(nameof(IsAway))]
    public bool IsAwayShown => IsAway && !HasLeft;

    [DependsOn(nameof(HasLeft))]
    [DependsOn(nameof(IsAway))]
    [DependsOn(nameof(HasOfferedDraw))]
    public bool IsDrawOfferShown => HasOfferedDraw && !IsAway && !HasLeft;

    [DependsOn(nameof(HasLeft))]
    [DependsOn(nameof(IsAway))]
    [DependsOn(nameof(HasOfferedDraw))]
    public bool IsDetailShown => HasLeft || (!IsAway && !HasOfferedDraw);

    public bool HasTurn
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public void MarkLeft(EndedMatchReason endReason)
    {
        _departureText = endReason == EndedMatchReason.Goal ? Strings.MatchPlayerAtGoal : Strings.MatchPlayerLeft;
        HasLeft = true;
    }

    public void ShowTurn(byte currentTurnOrder)
    {
        HasTurn = TurnOrder == currentTurnOrder;
    }

    public void HideTurn()
    {
        HasTurn = false;
    }

    public void ShowClock(TimeSpan remainingClock)
    {
        ClockText = MatchScreenText.FormatClock(remainingClock);
        IsClockLow = remainingClock <= _lowClockThreshold;
    }

    public void ShowConnection(bool isAway, TimeSpan timeAway)
    {
        IsAway = isAway;
        if (!isAway)
        {
            AwayText = string.Empty;
            return;
        }

        string timeAwayText = MatchScreenText.FormatElapsed(timeAway);
        AwayText = string.Format(CultureInfo.CurrentCulture, Strings.PlayerAwayFormat, timeAwayText);
    }

    public void ShowDrawOffer(bool hasOfferedDraw)
    {
        HasOfferedDraw = hasOfferedDraw;
    }

    public void ShowWalls(byte remainingWalls)
    {
        WallsText = MatchScreenText.FormatWallCount(remainingWalls);
    }
}
