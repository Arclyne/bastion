using System.Collections.Generic;
using Bastion.Server.Application.Chat;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeMatchChatNotifier : IMatchChatNotifier
{
    public FakeMatchChatNotifier()
    {
        NotifiedMessages = new List<NotifiedMatchChatMessage>();
    }

    public List<NotifiedMatchChatMessage> NotifiedMessages { get; }

    public void NotifyMatchChatMessage(
        int matchId,
        IReadOnlyCollection<int> recipientAccountIds,
        PostedChatLine postedLine)
    {
        var notifiedMessage = new NotifiedMatchChatMessage(matchId, recipientAccountIds, postedLine);
        NotifiedMessages.Add(notifiedMessage);
    }
}
