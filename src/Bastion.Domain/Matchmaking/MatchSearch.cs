using System;

namespace Bastion.Domain.Matchmaking;

public class MatchSearch
{
    public MatchSearch(int accountId, string nickname, DateTime startedAt)
    {
        ArgumentNullException.ThrowIfNull(nickname);

        AccountId = accountId;
        Nickname = nickname;
        StartedAt = startedAt;
        LastConfirmedAt = startedAt;
        Status = SearchStatus.Searching;
    }

    public int AccountId { get; }

    public string Nickname { get; }

    public DateTime StartedAt { get; }

    public DateTime LastConfirmedAt { get; private set; }

    public DateTime? PresenceRequestedAt { get; private set; }

    public SearchStatus Status { get; private set; }

    public bool IsAvailable()
    {
        return Status == SearchStatus.Searching;
    }

    public bool ShouldRequestPresence(DateTime now)
    {
        return IsAvailable() && now - LastConfirmedAt >= MatchmakingRules.PresenceCheckInterval;
    }

    public bool HasPresenceExpired(DateTime now)
    {
        return PresenceRequestedAt is DateTime requestedAt
            && now - requestedAt >= MatchmakingRules.PresenceResponseWindow;
    }

    public DateTime RequestPresence(DateTime now)
    {
        if (!IsAvailable())
        {
            throw new InvalidOperationException("Presence was already requested for this search.");
        }

        Status = SearchStatus.AwaitingPresence;
        PresenceRequestedAt = now;
        return now + MatchmakingRules.PresenceResponseWindow;
    }

    public void ConfirmPresence(DateTime now)
    {
        if (Status != SearchStatus.AwaitingPresence)
        {
            return;
        }

        Status = SearchStatus.Searching;
        PresenceRequestedAt = null;
        LastConfirmedAt = now;
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
