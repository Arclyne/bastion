using System;
using Bastion.Domain.Chat;
using Bastion.Server.Application.Matches;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Chat;

/// <summary>
/// Saves the messages that the participants of a match write in its chat and delivers them to the others.
/// </summary>
public interface IMatchChatService
{
    /// <summary>
    /// Moderates the message like the lobby chat, saves it in the match chat and delivers it to the other
    /// participants of the match.
    /// </summary>
    /// <param name="sessionToken">The token of the author's open session.</param>
    /// <param name="submission">The match and the text that the author wrote.</param>
    /// <param name="now">The current UTC time, saved as the moment the message was sent.</param>
    /// <returns>The saved message with its id.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="sessionToken"/> or <paramref name="submission"/> is null.
    /// </exception>
    /// <exception cref="InvalidSessionException">The session is not open.</exception>
    /// <exception cref="ChatChannelForbiddenException">
    /// The author does not take part in the unfinished match.
    /// </exception>
    /// <exception cref="ChatMessageRejectedException">
    /// The text is invalid or has a banned term, the author is muted, or the author writes too fast.
    /// </exception>
    /// <exception cref="MatchRepositoryException">The match repository could not be read or written.</exception>
    PostedChatLine SendMessage(string sessionToken, MatchChatSubmission submission, DateTime now);
}
