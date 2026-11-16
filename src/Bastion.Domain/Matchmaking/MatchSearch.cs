using System;

namespace Bastion.Domain.Matchmaking;

public class MatchSearch
{
    private DateTime _lastConfirmedAt;
    private DateTime? _presenceRequestedAt;
    private SearchStatus _status;

    public MatchSearch(int accountId, string nickname, DateTime startedAt)
    {
        ArgumentNullException.ThrowIfNull(nickname);

        AccountId = accountId;
        Nickname = nickname;
        StartedAt = startedAt;
        _lastConfirmedAt = startedAt;
        _status = SearchStatus.Searching;
    }

    public int AccountId { get; }

    public string Nickname { get; }

    public DateTime StartedAt { get; }

    public bool IsSearching()
    {
        return _status == SearchStatus.Searching;
    }

    public bool ShouldStartPresenceCheck(DateTime now)
    {
        TimeSpan timeSinceConfirmation = now - _lastConfirmedAt;
        return IsSearching() && timeSinceConfirmation >= MatchmakingPolicy.PresenceCheckInterval;
    }

    public bool HasPresenceExpired(DateTime now)
    {
        if (_presenceRequestedAt is not DateTime requestedAt)
        {
            return false;
        }

        TimeSpan waitingTime = now - requestedAt;
        return waitingTime >= MatchmakingPolicy.PresenceResponseWindow;
    }

    public DateTime StartPresenceCheck(DateTime now)
    {
        if (!IsSearching())
        {
            throw new InvalidOperationException("Presence was already requested for this search.");
        }

        _status = SearchStatus.AwaitingPresence;
        _presenceRequestedAt = now;
        DateTime presenceDeadline = now + MatchmakingPolicy.PresenceResponseWindow;
        return presenceDeadline;
    }

    public void ConfirmPresence(DateTime now)
    {
        if (_status != SearchStatus.AwaitingPresence)
        {
            return;
        }

        _status = SearchStatus.Searching;
        _presenceRequestedAt = null;
        _lastConfirmedAt = now;
    }

    public override bool Equals(object? obj)
    {
        return obj is MatchSearch other && other.AccountId == AccountId;
    }

    public override int GetHashCode()
    {
        return AccountId.GetHashCode();
    }
}
