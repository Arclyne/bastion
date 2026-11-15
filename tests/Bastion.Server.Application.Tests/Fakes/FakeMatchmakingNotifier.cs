using System.Collections.Generic;
using Bastion.Server.Application.Matchmaking;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeMatchmakingNotifier : IMatchmakingNotifier
{
    public HashSet<int> SubscribedAccountIds { get; } = new HashSet<int>();

    public List<CreatedMatch> CreatedMatches { get; } = new List<CreatedMatch>();

    public List<PresenceRequest> PresenceRequests { get; } = new List<PresenceRequest>();

    public Dictionary<int, SearchEndReason> EndReasons { get; } = new Dictionary<int, SearchEndReason>();

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
        EndReasons[accountId] = reason;
    }
}
