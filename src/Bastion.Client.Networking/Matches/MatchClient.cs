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

        try
        {
            _channel.Call(service => service.AbandonMatch(sessionToken, matchId));
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

    public void MovePawn(string sessionToken, ChosenPawnMove move)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(move);

        PawnMove contractMove = ToContractMove(move);
        try
        {
            _channel.Call(service => service.MovePawn(sessionToken, contractMove));
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

    public void PlaceWall(string sessionToken, ChosenWall wall)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(wall);

        var contractWall = MatchContractTranslator.ToWallPlacement(wall);
        try
        {
            _channel.Call(service => service.PlaceWall(sessionToken, contractWall));
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

    private static PawnMove ToContractMove(ChosenPawnMove move)
    {
        return new PawnMove
        {
            MatchId = move.MatchId,
            MoveNumber = move.MoveNumber,
            ToSquare = move.ToSquare,
        };
    }

    private static Exception ToClientException(Exception ex)
    {
        switch (ex)
        {
            case FaultException<MatchNotJoinedFault>:
                return new MatchNotJoinedException(ex);
            case FaultException<NotYourTurnFault> notYourTurn:
                return new NotYourTurnException(
                    notYourTurn.Detail.CurrentTurnOrder,
                    notYourTurn.Detail.NextMoveNumber,
                    ex);
            case FaultException<IllegalMoveFault>:
                return new IllegalMoveException(ex);
            case FaultException<IllegalWallFault> illegalWall:
                return MatchContractTranslator.ToIllegalWallException(illegalWall);
            default:
                return ServiceFailureTranslator.ToClientException(ex);
        }
    }
}
