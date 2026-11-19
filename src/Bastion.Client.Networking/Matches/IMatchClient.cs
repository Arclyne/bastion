using System;
using Bastion.Client.Networking.Notifications;

namespace Bastion.Client.Networking.Matches;

public interface IMatchClient
{
    event EventHandler<ParticipationEndedEventArgs>? ParticipationEnded;

    event EventHandler<MatchEndedEventArgs>? MatchEnded;

    event EventHandler<PawnMovedEventArgs>? PawnMoved;

    event EventHandler<WallPlacedEventArgs>? WallPlaced;

    event EventHandler<OpponentConnectionEventArgs>? OpponentDisconnected;

    event EventHandler<OpponentConnectionEventArgs>? OpponentReconnected;

    event EventHandler? MatchTakenOver;

    event EventHandler<DrawEventArgs>? DrawOffered;

    event EventHandler<DrawEventArgs>? DrawRejected;

    event EventHandler<DrawEventArgs>? DrawOfferExpired;

    void AbandonMatch(string sessionToken, int matchId);

    PreviewedAbandonment PreviewAbandonment(string sessionToken, int matchId);

    void MovePawn(string sessionToken, ChosenPawnMove chosenMove);

    void PlaceWall(string sessionToken, ChosenWall wall);

    FoundOngoingMatch FindOngoingMatch(string sessionToken);

    RejoinedMatch RejoinMatch(string sessionToken, int matchId);

    void OfferDraw(string sessionToken, int matchId);

    void RespondToDraw(string sessionToken, int matchId, bool isAccepted);
}
