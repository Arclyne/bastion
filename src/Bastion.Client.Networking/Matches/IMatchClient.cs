using System;
using Bastion.Client.Networking.Notifications;

namespace Bastion.Client.Networking.Matches;

public interface IMatchClient
{
    event EventHandler<MatchAbandonedEventArgs>? MatchAbandoned;

    event EventHandler<PawnMovedEventArgs>? PawnMoved;

    void AbandonMatch(string sessionToken, int matchId);

    void MovePawn(string sessionToken, ChosenPawnMove move);
}
