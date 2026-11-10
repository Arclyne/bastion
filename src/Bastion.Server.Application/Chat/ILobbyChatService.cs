using System;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Chat;

/// <summary>
/// Runs the lobby chat that every connected player shares: moderates and delivers the messages and gives the
/// recent history to the players who open it.
/// </summary>
public interface ILobbyChatService
{
    /// <summary>
    /// Moderates the message, checks that the author waited since the last one, and delivers it to everyone in the
    /// lobby chat.
    /// </summary>
    /// <param name="sessionToken">The token of the author's open session.</param>
    /// <param name="text">The text that the author wrote.</param>
    /// <param name="now">The current UTC time, kept as the moment the message was sent.</param>
    /// <returns>The delivered message with its id.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="sessionToken"/> or <paramref name="text"/> is null.
    /// </exception>
    /// <exception cref="InvalidSessionException">The session is not open.</exception>
    /// <exception cref="ChatMessageRejectedException">
    /// The text is invalid or has a banned term, the author is muted, or the author writes too fast.
    /// </exception>
    PostedChatLine SendMessage(string sessionToken, string text, DateTime now);

    /// <summary>
    /// Gives the player the recent messages of the lobby chat, how many players are listening, and whether the player
    /// is muted.
    /// </summary>
    /// <param name="sessionToken">The token of the player's open session.</param>
    /// <param name="now">The current UTC time, used to decide whether a mute is still in effect.</param>
    /// <returns>The recent messages, marked as own or not, the number of listeners and the player's mute.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sessionToken"/> is null.</exception>
    /// <exception cref="InvalidSessionException">The session is not open.</exception>
    LobbyChatHistory OpenLobbyChat(string sessionToken, DateTime now);
}
