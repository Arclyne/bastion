using System;
using System.Collections.Generic;
using Bastion.Server.Application.Chat;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeMatchChatNotifier : IMatchChatNotifier
{
    public List<NotifiedMatchChatMessage> NotifiedMessages { get; } = new List<NotifiedMatchChatMessage>();

    public void NotifyMatchChatMessage(
        int matchId,
        IReadOnlyCollection<int> recipientAccountIds,
        PostedChatLine postedLine)
    {
        ArgumentNullException.ThrowIfNull(recipientAccountIds);
        ArgumentNullException.ThrowIfNull(postedLine);

        NotifiedMessages.Add(new NotifiedMatchChatMessage(matchId, recipientAccountIds, postedLine));
    }
}
