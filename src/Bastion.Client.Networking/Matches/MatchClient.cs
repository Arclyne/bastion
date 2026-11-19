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

    public event EventHandler<ParticipationEndedEventArgs>? ParticipationEnded
    {
        add => _notificationClient.ParticipationEnded += value;
        remove => _notificationClient.ParticipationEnded -= value;
    }

    public event EventHandler<MatchEndedEventArgs>? MatchEnded
    {
        add => _notificationClient.MatchEnded += value;
        remove => _notificationClient.MatchEnded -= value;
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

        Action<IMatchService> abandonMatch = service => service.AbandonMatch(sessionToken, matchId);
        Call(abandonMatch);
    }

    public PreviewedAbandonment PreviewAbandonment(string sessionToken, int matchId)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        Func<IMatchService, AbandonmentPreview> previewAbandonment = service => service.PreviewAbandonment(
            sessionToken,
            matchId);
        AbandonmentPreview preview = Call(previewAbandonment);
        return MatchContractTranslator.ToPreviewedAbandonment(preview);
    }

    public void MovePawn(string sessionToken, ChosenPawnMove chosenMove)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(chosenMove);

        var contractMove = MatchContractTranslator.ToPawnMove(chosenMove);
        Action<IMatchService> movePawn = service => service.MovePawn(sessionToken, contractMove);
        Call(movePawn);
    }

    public void PlaceWall(string sessionToken, ChosenWall wall)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(wall);

        var contractWall = MatchContractTranslator.ToWallPlacement(wall);
        Action<IMatchService> placeWall = service => service.PlaceWall(sessionToken, contractWall);
        Call(placeWall);
    }

    private void Call(Action<IMatchService> operation)
    {
        Func<IMatchService, bool> completeOperation = service => CompleteOperation(operation, service);
        Call(completeOperation);
    }

    private static bool CompleteOperation(Action<IMatchService> operation, IMatchService service)
    {
        operation(service);
        return true;
    }

    private TResult Call<TResult>(Func<IMatchService, TResult> operation)
    {
        try
        {
            return _channel.Call(operation);
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
        if (cause is FaultException<MatchNotJoinedFault>)
        {
            return new MatchNotJoinedException(cause);
        }

        if (cause is FaultException<NotYourTurnFault> notYourTurn)
        {
            return new NotYourTurnException(
                notYourTurn.Detail.CurrentTurnOrder,
                notYourTurn.Detail.NextMoveNumber,
                cause);
        }

        if (cause is FaultException<IllegalMoveFault>)
        {
            return new IllegalMoveException(cause);
        }

        if (cause is FaultException<IllegalWallFault> illegalWall)
        {
            return MatchContractTranslator.ToIllegalWallException(illegalWall);
        }

        return ServiceFailureTranslator.ToClientException(cause);
    }
}
