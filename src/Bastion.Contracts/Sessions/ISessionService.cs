using System.ServiceModel;
using Bastion.Contracts.Notifications;

namespace Bastion.Contracts.Sessions;

/// <summary>
/// Opens the duplex channel through which the server sends notifications to a signed-in player.
/// </summary>
[ServiceContract(CallbackContract = typeof(INotificationCallback))]
public interface ISessionService
{
    /// <summary>
    /// Registers the caller's callback channel for the player of the session, replacing any earlier one.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <exception cref="FaultException{TDetail}">With <see cref="InvalidSessionFault"/>: the session is not open.</exception>
    /// <exception cref="FaultException">The server could not reach the database.</exception>
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    void Subscribe(string? sessionToken);

    /// <summary>
    /// Tells the server that the client is still there, so the duplex channel stays open while the player sends nothing else on it.
    /// </summary>
    [OperationContract(IsOneWay = true)]
    void KeepAlive();
}
