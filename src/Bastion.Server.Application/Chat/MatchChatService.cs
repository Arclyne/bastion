using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Chat;
using Bastion.Server.Application.Sessions;
using log4net;

namespace Bastion.Server.Application.Chat;

/// <inheritdoc/>
public sealed class MatchChatService : IMatchChatService
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(MatchChatService));

    private readonly ISessionVerifier _sessionVerifier;
    private readonly IChatModerator _chatModerator;
    private readonly IMatchChatRelay _matchChatRelay;

    public MatchChatService(
        ISessionVerifier sessionVerifier,
        IChatModerator chatModerator,
        IMatchChatRelay matchChatRelay)
    {
        ArgumentNullException.ThrowIfNull(sessionVerifier);
        ArgumentNullException.ThrowIfNull(chatModerator);
        ArgumentNullException.ThrowIfNull(matchChatRelay);

        _sessionVerifier = sessionVerifier;
        _chatModerator = chatModerator;
        _matchChatRelay = matchChatRelay;
    }

    /// <inheritdoc/>
    public PostedChatLine SendMessage(string sessionToken, MatchChatSubmission submission, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(submission);

        ConnectedPlayer author = _sessionVerifier.Verify(sessionToken, now);
        IReadOnlyList<int> participantAccountIds = _matchChatRelay.ListParticipantAccountIds(submission.MatchId);
        if (!participantAccountIds.Contains(author.AccountId))
        {
            _logger.Warn(
                $"A non-participant wrote in a match chat. AccountId={author.AccountId}; MatchId={submission.MatchId}");
            throw new ChatChannelForbiddenException();
        }

        string screenedText = _chatModerator.ScreenMessage(author.AccountId, submission.Text, now);
        if (!_matchChatRelay.TryStartCooldown(author.AccountId, now))
        {
            throw new ChatMessageRejectedException(ChatRejection.TooFast);
        }

        IEnumerable<int> otherParticipantAccountIds =
            participantAccountIds.Where(accountId => accountId != author.AccountId);
        var recipientAccountIds = new List<int>(otherParticipantAccountIds);
        var line = new ChatLine(author, screenedText, now);
        return _matchChatRelay.Post(submission.MatchId, line, recipientAccountIds);
    }
}
