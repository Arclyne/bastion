using System.ServiceModel;
using Bastion.Contracts.Sessions;

namespace Bastion.Contracts.Rooms;

/// <summary>
/// Opens private rooms and lets players join them with their code, get ready and leave.
/// The changes made by other members arrive through the notification channel opened with <see cref="ISessionService"/>.
/// </summary>
[ServiceContract]
public interface IRoomService
{
    /// <summary>
    /// Opens a private room hosted by the player of the session.
    /// </summary>
    /// <param name="sessionToken">The token of the host's open session.</param>
    /// <param name="options">The game mode, walls per player and clock chosen by the host.</param>
    /// <returns>The opened room, with its code and the host in the first seat.</returns>
    /// <exception cref="FaultException{TDetail}">With <see cref="InvalidSessionFault"/>: the session is not open.</exception>
    /// <exception cref="FaultException{TDetail}">With <see cref="RoomRejectedFault"/>: the room cannot open; the detail says why.</exception>
    /// <exception cref="FaultException">The server could not reach the database.</exception>
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    [FaultContract(typeof(RoomRejectedFault))]
    RoomState CreateRoom(string? sessionToken, RoomOptions? options);

    /// <summary>
    /// Seats the player of the session in the open room with the given code, not ready yet.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <param name="code">The six-character code of the room.</param>
    /// <returns>The room after the player took a seat.</returns>
    /// <exception cref="FaultException{TDetail}">With <see cref="InvalidSessionFault"/>: the session is not open.</exception>
    /// <exception cref="FaultException{TDetail}">With <see cref="RoomRejectedFault"/>: the player cannot join; the detail says why.</exception>
    /// <exception cref="FaultException">The server could not reach the database.</exception>
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    [FaultContract(typeof(RoomRejectedFault))]
    RoomState JoinRoom(string? sessionToken, string? code);

    /// <summary>
    /// Marks the player of the session as ready or not ready in their room.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <param name="isReady">Whether the player is ready to play.</param>
    /// <returns>The room after the change.</returns>
    /// <exception cref="FaultException{TDetail}">With <see cref="InvalidSessionFault"/>: the session is not open.</exception>
    /// <exception cref="FaultException{TDetail}">With <see cref="RoomRejectedFault"/>: the player is not in a room.</exception>
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    [FaultContract(typeof(RoomRejectedFault))]
    RoomState SetReady(string? sessionToken, bool isReady);

    /// <summary>
    /// Changes the settings of the room hosted by the player of the session. The other members stop being ready.
    /// </summary>
    /// <param name="sessionToken">The token of the host's open session.</param>
    /// <param name="options">The new game mode, walls per player and clock.</param>
    /// <returns>The room after the change.</returns>
    /// <exception cref="FaultException{TDetail}">With <see cref="InvalidSessionFault"/>: the session is not open.</exception>
    /// <exception cref="FaultException{TDetail}">With <see cref="RoomRejectedFault"/>: the settings cannot change; the detail says why.</exception>
    /// <exception cref="FaultException">The server could not reach the database.</exception>
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    [FaultContract(typeof(RoomRejectedFault))]
    RoomState ChangeOptions(string? sessionToken, RoomOptions? options);

    /// <summary>
    /// Takes the player of the session out of their room. When the host leaves, the room closes for everyone.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    [OperationContract(IsOneWay = true)]
    void LeaveRoom(string? sessionToken);

    /// <summary>
    /// Finds the room of the player of the session, so a client that lost a reply can show where the player is.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <returns>The room of the player, or null if the player is not in a room.</returns>
    /// <exception cref="FaultException{TDetail}">With <see cref="InvalidSessionFault"/>: the session is not open.</exception>
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    RoomState? FindOwnRoom(string? sessionToken);
}
