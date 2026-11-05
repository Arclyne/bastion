using System;
using Bastion.Domain.Matchmaking;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Matchmaking;

/// <inheritdoc/>
public sealed class MatchSearchService : IMatchSearchService
{
    private readonly ISessionVerifier _sessionVerifier;
    private readonly IMatchmakingQueue _matchmakingQueue;
    private readonly IMatchmakingNotifier _matchmakingNotifier;

    public MatchSearchService(ISessionVerifier sessionVerifier, IMatchmakingQueue matchmakingQueue, IMatchmakingNotifier matchmakingNotifier)
    {
        ArgumentNullException.ThrowIfNull(sessionVerifier);
        ArgumentNullException.ThrowIfNull(matchmakingQueue);
        ArgumentNullException.ThrowIfNull(matchmakingNotifier);

        _sessionVerifier = sessionVerifier;
        _matchmakingQueue = matchmakingQueue;
        _matchmakingNotifier = matchmakingNotifier;
    }

    /// <inheritdoc/>
    public void StartSearch(string sessionToken, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        ConnectedPlayer player = _sessionVerifier.Verify(sessionToken, now);
        if (!_matchmakingNotifier.HasNotificationChannel(player.AccountId))
        {
            throw new SearchRejectedException(SearchRejection.NotificationsUnavailable);
        }

        // A repeated request keeps the first search, so the player does not lose the time already waited.
        _matchmakingQueue.TryAdd(new MatchSearch(player.AccountId, player.Nickname, now));
    }

    /// <inheritdoc/>
    public void CancelSearch(string sessionToken, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        ConnectedPlayer player = _sessionVerifier.Verify(sessionToken, now);
        if (!_matchmakingQueue.TryRemove(player.AccountId))
        {
            return;
        }

        _matchmakingNotifier.NotifySearchEnded(player.AccountId, SearchEndReason.Cancelled);
    }

    /// <inheritdoc/>
    public void ConfirmPresence(string sessionToken, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        ConnectedPlayer player = _sessionVerifier.Verify(sessionToken, now);
        _matchmakingQueue.ConfirmPresence(player.AccountId, now);
    }
}
