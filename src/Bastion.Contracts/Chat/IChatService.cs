using System.ServiceModel;
using Bastion.Contracts.Sessions;

namespace Bastion.Contracts.Chat;

/// <summary>
/// Sends chat messages to the lobby chat and to the chat of a match.
/// The messages arrive through the notification channel opened with <see cref="ISessionService"/>.
/// </summary>
[ServiceContract]
public interface IChatService
{
    /// <summary>
    /// Sends a message to the lobby chat, which every player with a notification channel receives.
    /// </summary>
    /// <param name="sessionToken">The token of the author's open session.</param>
    /// <param name="text">The text of the message.</param>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="InvalidSessionFault"/>: the session is not open.
    /// </exception>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="InvalidChatMessageFault"/>: the text is empty or too long.
    /// </exception>
    /// <exception cref="FaultException">The server could not reach the database.</exception>
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    [FaultContract(typeof(InvalidChatMessageFault))]
    void SendLobbyChatMessage(string? sessionToken, string? text);

    /// <summary>
    /// Saves a message in the chat of a match and sends it to the other participants of the match.
    /// </summary>
    /// <param name="sessionToken">The token of the author's open session.</param>
    /// <param name="matchId">The unfinished match whose chat receives the message.</param>
    /// <param name="text">The text of the message.</param>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="InvalidSessionFault"/>: the session is not open.
    /// </exception>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="ChatChannelForbiddenFault"/>: the author does not take part in the unfinished match.
    /// </exception>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="InvalidChatMessageFault"/>: the text is empty or too long.
    /// </exception>
    /// <exception cref="FaultException">The server could not read or write the database.</exception>
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    [FaultContract(typeof(ChatChannelForbiddenFault))]
    [FaultContract(typeof(InvalidChatMessageFault))]
    void SendMatchChatMessage(string? sessionToken, int matchId, string? text);
}
