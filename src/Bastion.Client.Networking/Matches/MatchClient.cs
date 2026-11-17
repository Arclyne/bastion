using System;
using System.ServiceModel;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Notifications;
using Bastion.Contracts.Matches;

namespace Bastion.Client.Networking.Matches;

public sealed class MatchClient : IMatchClient
{
    private readonly IServiceChannel<IMatchService> _channel;
    private readonly INotificationClient _notificationClient;

    public MatchClient(IServiceChannel<IMatchService> channel, INotificationClient notificationClient)
    {
        ArgumentNullException.ThrowIfNull(channel);
        ArgumentNullException.ThrowIfNull(notificationClient);

        _channel = channel;
        _notificationClient = notificationClient;
    }

    public event EventHandler<MatchAbandonedEventArgs>? MatchAbandoned
    {
        add => _notificationClient.MatchAbandoned += value;
        remove => _notificationClient.MatchAbandoned -= value;
    }

    public event EventHandler<PawnMovedEventArgs>? PawnMoved
    {
        add => _notificationClient.PawnMoved += value;
        remove => _notificationClient.PawnMoved -= value;
    }

    public event EventHandler<WallPlacedEventArgs>? WallPlaced
    {
        add => _notificationClient.WallPlaced += value;
        remove => _notificationClient.WallPlaced -= value;
    }

    public void AbandonMatch(string sessionToken, int matchId)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        Call(service => service.AbandonMatch(sessionToken, matchId));
    }

    public void MovePawn(string sessionToken, ChosenPawnMove move)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(move);

        var contractMove = MatchContractTranslator.ToPawnMove(move);
        Call(service => service.MovePawn(sessionToken, contractMove));
    }

    public void PlaceWall(string sessionToken, ChosenWall wall)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(wall);

        var contractWall = MatchContractTranslator.ToWallPlacement(wall);
        Call(service => service.PlaceWall(sessionToken, contractWall));
    }

    private void Call(Action<IMatchService> operation)
    {
        try
        {
            _channel.Call(operation);
        }
        catch (CommunicationException ex)
        {
            throw ToClientException(ex);
        }
        catch (TimeoutException ex)
        {
            throw ToClientException(ex);
        }
    }

    private static Exception ToClientException(Exception cause)
    {
        switch (cause)
        {
            case FaultException<MatchNotJoinedFault>:
                return new MatchNotJoinedException(cause);
            case FaultException<NotYourTurnFault> notYourTurn:
                return new NotYourTurnException(
                    notYourTurn.Detail.CurrentTurnOrder,
                    notYourTurn.Detail.NextMoveNumber,
                    cause);
            case FaultException<IllegalMoveFault>:
                return new IllegalMoveException(cause);
            case FaultException<IllegalWallFault> illegalWall:
                return MatchContractTranslator.ToIllegalWallException(illegalWall);
            default:
                return ServiceFailureTranslator.ToClientException(cause);
        }
    }
}
