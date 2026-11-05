using System;
using Bastion.Client.Networking.Notifications;
using Bastion.Domain.Matches;

namespace Bastion.Client.Screens.Matches;

public sealed class MatchPlayerViewModel
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

    public bool HasFirstTurn()
    {
        return TurnOrder == Match.FirstTurnOrder;
    }
}
