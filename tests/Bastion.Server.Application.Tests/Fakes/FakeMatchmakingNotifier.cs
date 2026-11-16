using System.Collections.Generic;
using Bastion.Server.Application.Matchmaking;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeMatchmakingNotifier : IMatchmakingNotifier
{
    public FakeMatchmakingNotifier()
    {
        SubscribedAccountIds = new HashSet<int>();
        CreatedMatches = new List<CreatedMatch>();
        PresenceRequests = new List<PresenceRequest>();
        EndReasonsByAccountId = new Dictionary<int, SearchEndReason>();
    }

    public HashSet<int> SubscribedAccountIds { get; }

    public List<CreatedMatch> CreatedMatches { get; }

    public List<PresenceRequest> PresenceRequests { get; }

    public Dictionary<int, SearchEndReason> EndReasonsByAccountId { get; }

    public bool HasNotificationChannel(int accountId)
    {
        return SubscribedAccountIds.Contains(accountId);
    }

    public void NotifyMatchFound(CreatedMatch createdMatch)
    {
        CreatedMatches.Add(createdMatch);
    }

    public void NotifyPresenceRequested(PresenceRequest request)
    {
        PresenceRequests.Add(request);
    }

    public void NotifySearchEnded(int accountId, SearchEndReason reason)
    {
        EndReasonsByAccountId[accountId] = reason;
    }
}
