using System.ServiceModel;
using Bastion.Contracts.Chat;
using Bastion.Contracts.Sessions;

namespace Bastion.Contracts.PrivateChat;

/// <summary>
/// Runs the private conversations between friends: lists them, opens one, sends a message and marks a conversation as
/// read. A new message reaches the recipient through the notification channel opened with <see cref="ISessionService"/>.
/// </summary>
[ServiceContract]
public interface IConversationService
{
    /// <summary>
    /// Lists the player's conversations, newest first, each one with its partner, the last message and how many
    /// messages from the partner are unread.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <returns>The conversations of the player, newest first.</returns>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="InvalidSessionFault"/>: the session is not open.
    /// </exception>
    /// <exception cref="FaultException">The server could not reach the database.</exception>
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    ConversationEntry[] ListConversations(string? sessionToken);

    /// <summary>
    /// Opens the conversation with the player who has the given friend code, gives its recent messages and marks the
    /// partner's messages as read. A former friend can still be opened while the conversation has messages.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <param name="friendCode">The friend code of the partner, with or without the hyphen.</param>
    /// <returns>The partner, the recent messages, and whether the player is muted and until when.</returns>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="InvalidSessionFault"/>: the session is not open.
    /// </exception>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="ChatMessageRejectedFault"/>: the code is missing or no other player has it, or the partner is
    /// not a friend and the conversation has no messages.
    /// </exception>
    /// <exception cref="FaultException">The server could not read or write the database.</exception>
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    [FaultContract(typeof(ChatMessageRejectedFault))]
    PrivateConversationLog OpenConversation(string? sessionToken, string? friendCode);

    /// <summary>
    /// Moderates the message, saves it and sends it to the friend with the given friend code, who receives it as a
    /// notification.
    /// </summary>
    /// <param name="sessionToken">The token of the author's open session.</param>
    /// <param name="friendCode">The friend code of the recipient, with or without the hyphen.</param>
    /// <param name="text">The text of the message.</param>
    /// <returns>The id of the message and the server time at which it was sent.</returns>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="InvalidSessionFault"/>: the session is not open.
    /// </exception>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="ChatMessageRejectedFault"/>: the code is missing or no other player has it, the recipient is not
    /// a friend, the text is invalid or has a banned term, the author is muted, or the author writes too fast.
    /// </exception>
    /// <exception cref="FaultException">The server could not read or write the database.</exception>
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    [FaultContract(typeof(ChatMessageRejectedFault))]
    ChatMessageReceipt SendPrivateMessage(string? sessionToken, string? friendCode, string? text);

    /// <summary>
    /// Marks as read the messages that the player with the given friend code sent to the player.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <param name="friendCode">The friend code of the partner, with or without the hyphen.</param>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="InvalidSessionFault"/>: the session is not open.
    /// </exception>
    /// <exception cref="FaultException{TDetail}">
    /// With <see cref="ChatMessageRejectedFault"/>: the code is missing or no other player has it.
    /// </exception>
    /// <exception cref="FaultException">The server could not read or write the database.</exception>
    [OperationContract]
    [FaultContract(typeof(InvalidSessionFault))]
    [FaultContract(typeof(ChatMessageRejectedFault))]
    void MarkConversationRead(string? sessionToken, string? friendCode);
}
