using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Common;
using Bastion.Domain.Matchmaking;

namespace Bastion.Server.Application.Matchmaking;

public sealed class MatchmakingQueue : IMatchmakingQueue
{
    private readonly object _searchesLock = new object();
    private readonly Dictionary<int, MatchSearch> _searches = [];

    public bool TryAdd(MatchSearch matchSearch)
    {
        ArgumentNullException.ThrowIfNull(matchSearch);

        lock (_searchesLock)
        {
            return _searches.TryAdd(matchSearch.AccountId, matchSearch);
        }
    }

    public bool TryRemove(int accountId)
    {
        lock (_searchesLock)
        {
            return _searches.Remove(accountId);
        }
    }

    public bool IsSearching(int accountId)
    {
        lock (_searchesLock)
        {
            return _searches.ContainsKey(accountId);
        }
    }

    public Lookup<DateTime> FindSearchStart(int accountId)
    {
        lock (_searchesLock)
        {
            MatchSearch? matchSearch = _searches.GetValueOrDefault(accountId);
            if (matchSearch is null)
            {
                return Lookup.Missing<DateTime>($"AccountId={accountId}");
            }

            return Lookup.Found(matchSearch.StartedAt);
        }
    }

    public bool IsAwaitingPresence(int accountId)
    {
        lock (_searchesLock)
        {
            MatchSearch? matchSearch = _searches.GetValueOrDefault(accountId);
            if (matchSearch is null)
            {
                return false;
            }

            bool isAwaitingPresence = !matchSearch.IsSearching();
            return isAwaitingPresence;
        }
    }

    public void ConfirmPresence(int accountId, DateTime now)
    {
        lock (_searchesLock)
        {
            MatchSearch? matchSearch = _searches.GetValueOrDefault(accountId);
            if (matchSearch is null)
            {
                return;
            }

            matchSearch.ConfirmPresence(now);
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
        IEnumerable<MatchSearch> unansweredSearches = _searches.Values
            .Where(matchSearch => matchSearch.HasPresenceExpired(now));
        List<MatchSearch> expiredSearches = [.. unansweredSearches];
        RemoveAll(expiredSearches);
        return expiredSearches;
    }

    private void RemoveAll(IEnumerable<MatchSearch> searches)
    {
        foreach (MatchSearch matchSearch in searches)
        {
            _searches.Remove(matchSearch.AccountId);
        }
    }

    private List<MatchPairing> TakePairings()
    {
        IEnumerable<MatchPairing> completePairings = _searches.Values
            .Where(matchSearch => matchSearch.IsSearching())
            .OrderBy(matchSearch => matchSearch.StartedAt)
            .Chunk(MatchmakingPolicy.PlayersPerMatch)
            .Where(players => players.Length == MatchmakingPolicy.PlayersPerMatch)
            .Select(players => new MatchPairing(players));
        List<MatchPairing> pairings = [.. completePairings];
        foreach (MatchPairing pairing in pairings)
        {
            RemoveAll(pairing.Players);
        }

        return pairings;
    }

    private List<PresenceRequest> StartPresenceChecks(DateTime now)
    {
        IEnumerable<MatchSearch> idleSearches = _searches.Values
            .Where(matchSearch => matchSearch.ShouldStartPresenceCheck(now));
        var presenceRequests = new List<PresenceRequest>();
        foreach (MatchSearch matchSearch in idleSearches)
        {
            DateTime presenceDeadline = matchSearch.StartPresenceCheck(now);
            var presenceRequest = new PresenceRequest(matchSearch.AccountId, presenceDeadline);
            presenceRequests.Add(presenceRequest);
        }

        return presenceRequests;
    }
}
