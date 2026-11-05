using System;
using Bastion.Domain.Matchmaking;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Matchmaking;

/// <inheritdoc/>
public sealed class MatchSearchService : IMatchSearchService
{
    private readonly IMatchmakingPlayerVerifier _playerVerifier;
    private readonly IMatchmakingQueue _matchmakingQueue;
    private readonly IMatchmakingNotifier _matchmakingNotifier;

    public MatchSearchService(
        IMatchmakingPlayerVerifier playerVerifier,
        IMatchmakingQueue matchmakingQueue,
        IMatchmakingNotifier matchmakingNotifier)
    {
        ArgumentNullException.ThrowIfNull(playerVerifier);
        ArgumentNullException.ThrowIfNull(matchmakingQueue);
        ArgumentNullException.ThrowIfNull(matchmakingNotifier);

        _playerVerifier = playerVerifier;
        _matchmakingQueue = matchmakingQueue;
        _matchmakingNotifier = matchmakingNotifier;
    }

    /// <inheritdoc/>
    public void StartSearch(string sessionToken, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        ConnectedPlayer player = _playerVerifier.VerifyPlayerCanSearch(sessionToken, now);

        // A repeated request keeps the first search, so the player does not lose the time already waited.
        _matchmakingQueue.TryAdd(new MatchSearch(player.AccountId, player.Nickname, now));
    }

    /// <inheritdoc/>
    public void CancelSearch(string sessionToken, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        ConnectedPlayer player = _playerVerifier.VerifySession(sessionToken, now);
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

        ConnectedPlayer player = _playerVerifier.VerifySession(sessionToken, now);
        _matchmakingQueue.ConfirmPresence(player.AccountId, now);
    }
}
