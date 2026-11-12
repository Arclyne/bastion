using System;
using Bastion.Client.Networking.Notifications;

namespace Bastion.Client.Networking.Matches;

public interface IMatchClient
{
    event EventHandler<MatchAbandonedEventArgs>? MatchAbandoned;

    event EventHandler<PawnMovedEventArgs>? PawnMoved;

    event EventHandler<WallPlacedEventArgs>? WallPlaced;

    void AbandonMatch(string sessionToken, int matchId);

    void MovePawn(string sessionToken, ChosenPawnMove move);

    void PlaceWall(string sessionToken, ChosenWall wall);
}
