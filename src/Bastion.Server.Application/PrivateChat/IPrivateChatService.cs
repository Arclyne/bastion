using System;
using System.Collections.Generic;
using Bastion.Domain.Chat;
using Bastion.Server.Application.Chat;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.PrivateChat;

/// <summary>
/// Runs the private conversations between friends: lists them, opens one, sends a message and marks a conversation as
/// read.
/// </summary>
public interface IPrivateChatService
{
    /// <summary>
    /// Lists the player's conversations, newest first, each one with its partner, the last message and how many
    /// messages from the partner are unread.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <param name="now">The current UTC time.</param>
    /// <returns>The conversations of the player, newest first.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sessionToken"/> is null.</exception>
    /// <exception cref="InvalidSessionException">The session is not open.</exception>
    IReadOnlyList<ConversationSummary> ListConversations(string sessionToken, DateTime now);

    /// <summary>
    /// Opens the conversation with the player who has the given friend code, gives its recent messages and marks the
    /// partner's messages as read. A former friend can still be opened while the conversation has messages.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <param name="friendCode">The friend code of the partner, with or without the hyphen.</param>
    /// <param name="now">The current UTC time, saved as the moment the messages were read.</param>
    /// <returns>The partner, the recent messages and whether the player is muted.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="sessionToken"/> or <paramref name="friendCode"/> is null.
    /// </exception>
    /// <exception cref="InvalidSessionException">The session is not open.</exception>
    /// <exception cref="ChatMessageRejectedException">
    /// No other player has the code, or the partner is not a friend and the conversation has no messages.
    /// </exception>
    PrivateConversation OpenConversation(string sessionToken, string friendCode, DateTime now);

    /// <summary>
    /// Moderates the message, saves it and delivers it to the friend with the friend code of the draft.
    /// </summary>
    /// <param name="sessionToken">The token of the author's open session.</param>
    /// <param name="draft">The friend code of the recipient and the text the author wrote.</param>
    /// <param name="now">The current UTC time, saved as the moment the message was sent.</param>
    /// <returns>The saved message.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="sessionToken"/> or <paramref name="draft"/> is null.
    /// </exception>
    /// <exception cref="InvalidSessionException">The session is not open.</exception>
    /// <exception cref="ChatMessageRejectedException">
    /// No other player has the code, the recipient is not a friend, the text is invalid or has a banned term, the
    /// author is muted, or the author writes too fast.
    /// </exception>
    Message SendPrivateMessage(string sessionToken, PrivateMessageDraft draft, DateTime now);

    /// <summary>
    /// Marks as read the messages that the player with the given friend code sent to the player.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <param name="friendCode">The friend code of the partner, with or without the hyphen.</param>
    /// <param name="now">The current UTC time, saved as the moment the messages were read.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="sessionToken"/> or <paramref name="friendCode"/> is null.
    /// </exception>
    /// <exception cref="InvalidSessionException">The session is not open.</exception>
    /// <exception cref="ChatMessageRejectedException">No other player has the code.</exception>
    void MarkConversationRead(string sessionToken, string friendCode, DateTime now);
}
