using System;
using Bastion.Server.Application.Matches;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Matchmaking;

public sealed class MatchmakingPlayerVerifier : IMatchmakingPlayerVerifier
{
    private readonly ISessionVerifier _sessionVerifier;
    private readonly IMatchmakingNotifier _matchmakingNotifier;
    private readonly IMatchStore _matchStore;

    public MatchmakingPlayerVerifier(
        ISessionVerifier sessionVerifier,
        IMatchmakingNotifier matchmakingNotifier,
        IMatchStore matchStore)
    {
        ArgumentNullException.ThrowIfNull(sessionVerifier);
        ArgumentNullException.ThrowIfNull(matchmakingNotifier);
        ArgumentNullException.ThrowIfNull(matchStore);

        _sessionVerifier = sessionVerifier;
        _matchmakingNotifier = matchmakingNotifier;
        _matchStore = matchStore;
    }

    public ConnectedPlayer VerifySession(string sessionToken, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        return _sessionVerifier.Verify(sessionToken, now);
    }

    public ConnectedPlayer VerifyPlayerCanSearch(string sessionToken, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        ConnectedPlayer player = VerifySession(sessionToken, now);
        if (!_matchmakingNotifier.HasNotificationChannel(player.AccountId))
        {
            throw new SearchRejectedException(SearchRejection.NotificationsUnavailable);
        }

        // A player takes part in one unfinished match at most (CU-09 RN-01, D-01).
        if (_matchStore.HasUnfinishedParticipation(player.AccountId))
        {
            throw new SearchRejectedException(SearchRejection.UnfinishedMatch);
        }

        return player;
    }
}
