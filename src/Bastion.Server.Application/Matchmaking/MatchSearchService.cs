using System;
using Bastion.Domain.Matchmaking;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Matchmaking;

/// <inheritdoc/>
public sealed class MatchSearchService : IMatchSearchService
{
    private readonly ISessionVerifier _sessionVerifier;
    private readonly IMatchmakingQueue _queue;
    private readonly IMatchmakingNotifier _notifier;

    public MatchSearchService(ISessionVerifier sessionVerifier, IMatchmakingQueue queue, IMatchmakingNotifier notifier)
    {
        ArgumentNullException.ThrowIfNull(sessionVerifier);
        ArgumentNullException.ThrowIfNull(queue);
        ArgumentNullException.ThrowIfNull(notifier);

        _sessionVerifier = sessionVerifier;
        _queue = queue;
        _notifier = notifier;
    }

    /// <inheritdoc/>
    public void Start(string sessionToken, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        ConnectedPlayer player = _sessionVerifier.Verify(sessionToken, now);
        if (!_notifier.HasNotificationChannel(player.AccountId))
        {
            throw new SearchRejectedException(SearchRejection.NotificationsUnavailable);
        }

        // A repeated request keeps the first search, so the player does not lose the time already waited.
        _queue.TryAdd(new MatchSearch(player.AccountId, player.Nickname, now));
    }

    /// <inheritdoc/>
    public void Cancel(string sessionToken, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        ConnectedPlayer player = _sessionVerifier.Verify(sessionToken, now);
        if (!_queue.TryRemove(player.AccountId))
        {
            return;
        }

        _notifier.NotifySearchEnded(player.AccountId, SearchEndReason.Cancelled);
    }

    /// <inheritdoc/>
    public void ConfirmPresence(string sessionToken, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        ConnectedPlayer player = _sessionVerifier.Verify(sessionToken, now);
        _queue.ConfirmPresence(player.AccountId, now);
    }
}
