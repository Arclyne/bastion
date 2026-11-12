using System;
using Bastion.Server.Application.Common;
using Bastion.Server.Application.Matches;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Matchmaking;

public sealed class MatchmakingPlayerVerifier : IMatchmakingPlayerVerifier
{
    private readonly ISessionVerifier _sessionVerifier;
    private readonly IPlayerActivity _playerActivity;
    private readonly IMatchRepository _matchRepository;

    public MatchmakingPlayerVerifier(
        ISessionVerifier sessionVerifier,
        IPlayerActivity playerActivity,
        IMatchRepository matchRepository)
    {
        ArgumentNullException.ThrowIfNull(sessionVerifier);
        ArgumentNullException.ThrowIfNull(playerActivity);
        ArgumentNullException.ThrowIfNull(matchRepository);

        _sessionVerifier = sessionVerifier;
        _playerActivity = playerActivity;
        _matchRepository = matchRepository;
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
        if (!_playerActivity.HasNotificationChannel(player.AccountId))
        {
            throw new SearchRejectedException(SearchRejection.NotificationsUnavailable);
        }

        if (_matchRepository.HasUnfinishedParticipation(player.AccountId))
        {
            throw new SearchRejectedException(SearchRejection.UnfinishedMatch);
        }

        if (_playerActivity.IsInRoom(player.AccountId))
        {
            throw new SearchRejectedException(SearchRejection.InRoom);
        }

        return player;
    }
}
