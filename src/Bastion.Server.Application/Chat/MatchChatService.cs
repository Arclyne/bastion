using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Chat;
using Bastion.Server.Application.Matches;
using Bastion.Server.Application.Sessions;
using log4net;

namespace Bastion.Server.Application.Chat;

/// <inheritdoc/>
public sealed class MatchChatService : IMatchChatService
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(MatchChatService));

    private readonly ISessionVerifier _sessionVerifier;
    private readonly IMatchStore _matchStore;
    private readonly IMatchChatNotifier _matchChatNotifier;

    public MatchChatService(
        ISessionVerifier sessionVerifier,
        IMatchStore matchStore,
        IMatchChatNotifier matchChatNotifier)
    {
        ArgumentNullException.ThrowIfNull(sessionVerifier);
        ArgumentNullException.ThrowIfNull(matchStore);
        ArgumentNullException.ThrowIfNull(matchChatNotifier);

        _sessionVerifier = sessionVerifier;
        _matchStore = matchStore;
        _matchChatNotifier = matchChatNotifier;
    }

    /// <inheritdoc/>
    public void SendMessage(string sessionToken, MatchChatSubmission submission, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(submission);

        ConnectedPlayer author = _sessionVerifier.Verify(sessionToken, now);
        IReadOnlyList<int> participantAccountIds =
            _matchStore.FindAccountIdsWithUnfinishedParticipation(submission.MatchId);
        if (!participantAccountIds.Contains(author.AccountId))
        {
            _logger.Warn(
                $"A non-participant wrote in a match chat. AccountId={author.AccountId}; MatchId={submission.MatchId}");
            throw new ChatChannelForbiddenException();
        }

        if (!ChatRules.IsValidMessage(submission.Text))
        {
            throw new InvalidChatMessageException();
        }

        // The message is saved before it is delivered, so a failed save reaches nobody (CU-20 EX-01).
        var message = ChatMessage.WriteInMatch(submission, author.AccountId, now);
        _matchStore.SaveChatMessage(message);

        IEnumerable<int> otherParticipantAccountIds =
            participantAccountIds.Where(accountId => accountId != author.AccountId);
        var recipientAccountIds = new List<int>(otherParticipantAccountIds);
        var line = new ChatLine(author.Nickname, message.Text, message.SentAt);
        _matchChatNotifier.NotifyMatchChatMessage(submission.MatchId, recipientAccountIds, line);
    }
}
