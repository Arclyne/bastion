using System;
using System.Collections.Generic;
using Bastion.Domain.Chat;
using Bastion.Server.Application.Matches;

namespace Bastion.Server.Application.Chat;

public sealed class MatchChatRelay : IMatchChatRelay
{
    private readonly ChatCooldown _cooldown = new ChatCooldown();
    private readonly IMatchRepository _matchRepository;
    private readonly IMatchChatNotifier _matchChatNotifier;

    public MatchChatRelay(IMatchRepository matchRepository, IMatchChatNotifier matchChatNotifier)
    {
        ArgumentNullException.ThrowIfNull(matchRepository);
        ArgumentNullException.ThrowIfNull(matchChatNotifier);

        _matchRepository = matchRepository;
        _matchChatNotifier = matchChatNotifier;
    }

    public IReadOnlyList<int> ListParticipantAccountIds(int matchId)
    {
        return _matchRepository.FindAccountIdsWithUnfinishedParticipation(matchId);
    }

    public bool TryStartCooldown(int accountId, DateTime now)
    {
        return _cooldown.TryStartCooldown(accountId, now);
    }

    public PostedChatLine Post(int matchId, ChatLine line, IReadOnlyCollection<int> recipientAccountIds)
    {
        ArgumentNullException.ThrowIfNull(line);
        ArgumentNullException.ThrowIfNull(recipientAccountIds);

        var submission = new MatchChatSubmission(matchId, line.Text);
        var message = Message.CreateForMatch(submission, line.Sender.AccountId, line.SentAt);
        long messageId = _matchRepository.SaveMatchChatMessage(message);
        var postedLine = new PostedChatLine(messageId, line);
        _matchChatNotifier.NotifyMatchChatMessage(matchId, recipientAccountIds, postedLine);
        return postedLine;
    }
}
