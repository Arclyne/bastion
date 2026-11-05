using System.ServiceModel;
using Bastion.Contracts.Sessions;

namespace Bastion.Contracts.Chat;

/// <summary>
/// Sends chat messages to the lobby chat and to the chat of a match, and opens the lobby chat.
/// New messages arrive through the notification channel opened with <see cref="ISessionService"/>.
/// </summary>
[ServiceContract]
public interface IChatService
{
    /// <summary>
    /// Sends a message to the lobby chat, which every other player with a notification channel receives.
    /// </summary>
    /// <param name="sessionToken">The token of the author's open session.</param>
    /// <param name="text">The text of the message.</param>
    /// <returns>The id of the message and the server time at which it was sent.</returns>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="InvalidSessionFault"/>: the session is not open.
    /// </exception>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="ChatMessageRejectedFault"/>: the text is invalid or has a banned term, the author is muted,
    /// or the author writes too fast.
    /// </exception>
    /// <exception cref="FaultException">The server could not reach the database.</exception>
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    [FaultContract(typeof(ChatMessageRejectedFault))]
    ChatMessageReceipt SendChatMessage(string? sessionToken, string? text);

    /// <summary>
    /// Opens the lobby chat for the player of the session.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <returns>
    /// The recent messages, the number of connected players, and whether the player is muted and until when.
    /// </returns>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="InvalidSessionFault"/>: the session is not open.
    /// </exception>
    /// <exception cref="FaultException">The server could not reach the database.</exception>
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    LobbyChatLog OpenLobbyChat(string? sessionToken);

    /// <summary>
    /// Saves a message in the chat of a match and sends it to the other participants of the match.
    /// </summary>
    /// <param name="sessionToken">The token of the author's open session.</param>
    /// <param name="matchId">The unfinished match whose chat receives the message.</param>
    /// <param name="text">The text of the message.</param>
    /// <returns>The id of the message and the server time at which it was sent.</returns>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="InvalidSessionFault"/>: the session is not open.
    /// </exception>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="ChatChannelForbiddenFault"/>: the author does not take part in the unfinished match.
    /// </exception>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="ChatMessageRejectedFault"/>: the text is invalid or has a banned term, the author is muted,
    /// or the author writes too fast.
    /// </exception>
    /// <exception cref="FaultException">The server could not read or write the database.</exception>
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    [FaultContract(typeof(ChatChannelForbiddenFault))]
    [FaultContract(typeof(ChatMessageRejectedFault))]
    ChatMessageReceipt SendMatchChatMessage(string? sessionToken, int matchId, string? text);
}
