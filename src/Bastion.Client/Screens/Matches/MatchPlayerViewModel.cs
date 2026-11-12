using System;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Notifications;
using Bastion.Domain.Matches;
using Gum.Mvvm;

namespace Bastion.Client.Screens.Matches;

public sealed class MatchPlayerViewModel : ViewModel
{
    public MatchPlayerViewModel(FoundMatchParticipant participant, FoundMatchFormat format, bool isOwn)
    {
        ArgumentNullException.ThrowIfNull(participant);
        ArgumentNullException.ThrowIfNull(format);

        Nickname = participant.Nickname;
        EloText = MatchScreenText.FormatElo(participant.EloRating);
        WallsText = MatchScreenText.FormatWallCount(format.WallsPerPlayer);
        ClockText = MatchScreenText.FormatClock(format.ClockMinutes);
        PawnSymbol = Participation.GetPawnSymbol(participant.TurnOrder);
        TurnOrder = participant.TurnOrder;
        IsOwn = isOwn;
    }

    public string Nickname { get; }

    public string EloText { get; }

    public string WallsText { get; }

    public string ClockText { get; }

    public PawnSymbol PawnSymbol { get; }

    public byte TurnOrder { get; }

    public bool IsOwn { get; }

    public bool HasLeft
    {
        get => Get<bool>();
        private set => Set(value);
    }

    [DependsOn(nameof(HasLeft))]
    public string DetailText => HasLeft ? Strings.MatchPlayerLeft : EloText;

    public bool HasTurn
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public void MarkLeft()
    {
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
}
