using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Matchmaking;

namespace Bastion.Server.Application.Matchmaking;

public sealed class MatchmakingQueue : IMatchmakingQueue
{
    private readonly object _searchesLock = new object();
    private readonly Dictionary<int, MatchSearch> _searches = new Dictionary<int, MatchSearch>();

    public bool TryAdd(MatchSearch search)
    {
        ArgumentNullException.ThrowIfNull(search);

        lock (_searchesLock)
        {
            return _searches.TryAdd(search.AccountId, search);
        }
    }

    public bool TryRemove(int accountId)
    {
        lock (_searchesLock)
        {
            return _searches.Remove(accountId);
        }
    }

    public void ConfirmPresence(int accountId, DateTime now)
    {
        lock (_searchesLock)
        {
            if (_searches.TryGetValue(accountId, out MatchSearch? search))
            {
                search.ConfirmPresence(now);
            }
        }
    }

    public QueueReview Review(DateTime now)
    {
        lock (_searchesLock)
        {
            List<MatchSearch> expiredSearches = RemoveExpiredSearches(now);
            List<MatchPairing> pairings = TakePairings();
            List<PresenceRequest> presenceRequests = StartPresenceChecks(now);
            return new QueueReview(expiredSearches, pairings, presenceRequests);
        }
    }

    private List<MatchSearch> RemoveExpiredSearches(DateTime now)
    {
        IEnumerable<MatchSearch> unansweredSearches = _searches.Values.Where(search => search.HasPresenceExpired(now));
        var expiredSearches = new List<MatchSearch>(unansweredSearches);
        RemoveAll(expiredSearches);
        return expiredSearches;
    }

    private List<MatchPairing> TakePairings()
    {
        IEnumerable<MatchPairing> completePairings = _searches.Values
            .Where(search => search.IsAvailable())
            .OrderBy(search => search.StartedAt)
            .Chunk(MatchmakingRules.PlayersPerMatch)
            .Where(players => players.Length == MatchmakingRules.PlayersPerMatch)
            .Select(players => new MatchPairing(players));
        var pairings = new List<MatchPairing>(completePairings);
        foreach (MatchPairing pairing in pairings)
        {
            RemoveAll(pairing.Players);
        }

        return pairings;
    }

    private List<PresenceRequest> StartPresenceChecks(DateTime now)
    {
        IEnumerable<MatchSearch> idleSearches = _searches.Values.Where(search => search.ShouldStartPresenceCheck(now));
        var presenceRequests = new List<PresenceRequest>();
        foreach (MatchSearch search in idleSearches)
        {
            presenceRequests.Add(new PresenceRequest(search.AccountId, search.StartPresenceCheck(now)));
        }

        return presenceRequests;
    }

    private void RemoveAll(IEnumerable<MatchSearch> searches)
    {
        foreach (MatchSearch search in searches)
        {
            _searches.Remove(search.AccountId);
        }
    }
}
