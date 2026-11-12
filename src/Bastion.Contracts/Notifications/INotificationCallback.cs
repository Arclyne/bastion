using System.ServiceModel;

namespace Bastion.Contracts.Notifications;

/// <summary>
/// Receives on the client the notifications that the server sends through the duplex channel opened with
/// <see cref="Sessions.ISessionService.Subscribe"/>.
/// </summary>
[ServiceContract]
public interface INotificationCallback
{
    /// <summary>
    /// Receives one notification. The call is one-way, so the server does not wait for the client.
    /// </summary>
    /// <param name="notification">The notification, of one of the types that <see cref="Notification"/> knows.</param>
    [OperationContract(IsOneWay = true)]
    void ReceiveNotification(Notification? notification);
}
