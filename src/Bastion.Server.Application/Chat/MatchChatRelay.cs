using System;
using System.Collections.Generic;
using Bastion.Domain.Chat;
using Bastion.Server.Application.Matches;

namespace Bastion.Server.Application.Chat;

public sealed class MatchChatRelay : IMatchChatRelay
{
    private readonly ChatCooldown _cooldown = new ChatCooldown();
    private readonly IMatchStore _matchStore;
    private readonly IMatchChatNotifier _matchChatNotifier;

    public MatchChatRelay(IMatchStore matchStore, IMatchChatNotifier matchChatNotifier)
    {
        ArgumentNullException.ThrowIfNull(matchStore);
        ArgumentNullException.ThrowIfNull(matchChatNotifier);

        _matchStore = matchStore;
        _matchChatNotifier = matchChatNotifier;
    }

    public IReadOnlyList<int> ListParticipantAccountIds(int matchId)
    {
        return _matchStore.FindAccountIdsWithUnfinishedParticipation(matchId);
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
        long messageId = _matchStore.SaveMatchChatMessage(message);
        var postedLine = new PostedChatLine(messageId, line);
        _matchChatNotifier.NotifyMatchChatMessage(matchId, recipientAccountIds, postedLine);
        return postedLine;
    }
}
