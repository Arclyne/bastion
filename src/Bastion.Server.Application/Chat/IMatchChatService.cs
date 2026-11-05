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
    /// Saves the message in the match chat and delivers it to the other participants of the match.
    /// </summary>
    /// <param name="sessionToken">The token of the author's open session.</param>
    /// <param name="submission">The match and the text that the author wrote.</param>
    /// <param name="now">The current UTC time, saved as the moment the message was sent.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="sessionToken"/> or <paramref name="submission"/> is null.
    /// </exception>
    /// <exception cref="InvalidSessionException">The session is not open.</exception>
    /// <exception cref="ChatChannelForbiddenException">
    /// The author does not take part in the unfinished match.
    /// </exception>
    /// <exception cref="InvalidChatMessageException">The text is empty or too long.</exception>
    /// <exception cref="MatchStoreException">The match store could not be read or written.</exception>
    void SendMessage(string sessionToken, MatchChatSubmission submission, DateTime now);
}
