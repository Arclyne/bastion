using System.ServiceModel;
using Bastion.Contracts.Sessions;

namespace Bastion.Contracts.Rooms;

/// <summary>
/// Opens private rooms and lets players join them with their code, get ready and leave, and lets the host change the
/// options and start the match.
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
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="InvalidSessionFault"/>: the session is not open.
    /// </exception>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="RoomRejectedFault"/>: the room cannot open; the detail says why.
    /// </exception>
    /// <exception cref="FaultException">The server could not read or write the database.</exception>
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
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="InvalidSessionFault"/>: the session is not open.
    /// </exception>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="RoomRejectedFault"/>: the player cannot join; the detail says why.
    /// </exception>
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
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="InvalidSessionFault"/>: the session is not open.
    /// </exception>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="RoomRejectedFault"/>: the player is not in a room.
    /// </exception>
    /// <exception cref="FaultException">The server could not reach the database.</exception>
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    [FaultContract(typeof(RoomRejectedFault))]
    RoomState SetReady(string? sessionToken, bool isReady);

    /// <summary>
    /// Changes the options of the room hosted by the player of the session. The other members stop being ready.
    /// </summary>
    /// <param name="sessionToken">The token of the host's open session.</param>
    /// <param name="options">The new game mode, walls per player and clock.</param>
    /// <returns>The room after the change.</returns>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="InvalidSessionFault"/>: the session is not open.
    /// </exception>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="RoomRejectedFault"/>: the options cannot change; the detail says why.
    /// </exception>
    /// <exception cref="FaultException">The server could not read or write the database.</exception>
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    [FaultContract(typeof(RoomRejectedFault))]
    RoomState ChangeOptions(string? sessionToken, RoomOptions? options);

    /// <summary>
    /// Starts the private match of the room hosted by the player of the session. Every member, the host included,
    /// receives the match-found notification; the room leaves the waiting state.
    /// </summary>
    /// <param name="sessionToken">The token of the host's open session.</param>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="InvalidSessionFault"/>: the session is not open.
    /// </exception>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="RoomRejectedFault"/>: the match cannot start; the detail says why.
    /// </exception>
    /// <exception cref="FaultException">
    /// The server could not reach the database or save the match; the room stays open.
    /// </exception>
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    [FaultContract(typeof(RoomRejectedFault))]
    void StartMatch(string? sessionToken);

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
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="InvalidSessionFault"/>: the session is not open.
    /// </exception>
    /// <exception cref="FaultException">The server could not reach the database.</exception>
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    RoomState? FindOwnRoom(string? sessionToken);
}
