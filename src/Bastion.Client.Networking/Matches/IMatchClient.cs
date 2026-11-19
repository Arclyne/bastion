using System;
using Bastion.Client.Networking.Notifications;

namespace Bastion.Client.Networking.Matches;

public interface IMatchClient
{
    event EventHandler<ParticipationEndedEventArgs>? ParticipationEnded;

    event EventHandler<MatchEndedEventArgs>? MatchEnded;

    event EventHandler<PawnMovedEventArgs>? PawnMoved;

    event EventHandler<WallPlacedEventArgs>? WallPlaced;

    void AbandonMatch(string sessionToken, int matchId);

    PreviewedAbandonment PreviewAbandonment(string sessionToken, int matchId);

    void MovePawn(string sessionToken, ChosenPawnMove chosenMove);

    void PlaceWall(string sessionToken, ChosenWall wall);
}
