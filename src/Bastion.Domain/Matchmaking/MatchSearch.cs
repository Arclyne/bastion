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

    public bool IsSearching()
    {
        return Status == SearchStatus.Searching;
    }

    public bool ShouldStartPresenceCheck(DateTime now)
    {
        TimeSpan timeSinceConfirmation = now - LastConfirmedAt;
        return IsSearching() && timeSinceConfirmation >= MatchmakingPolicy.PresenceCheckInterval;
    }

    public bool HasPresenceExpired(DateTime now)
    {
        if (PresenceRequestedAt is not DateTime requestedAt)
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

        Status = SearchStatus.AwaitingPresence;
        PresenceRequestedAt = now;
        DateTime presenceDeadline = now + MatchmakingPolicy.PresenceResponseWindow;
        return presenceDeadline;
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
