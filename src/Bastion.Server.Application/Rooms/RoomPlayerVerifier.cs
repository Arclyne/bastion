using System;
using Bastion.Server.Application.Common;
using Bastion.Server.Application.Matches;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Rooms;

public sealed class RoomPlayerVerifier : IRoomPlayerVerifier
{
    private readonly ISessionVerifier _sessionVerifier;
    private readonly IMatchStore _matchStore;
    private readonly IPlayerActivity _playerActivity;

    public RoomPlayerVerifier(ISessionVerifier sessionVerifier, IMatchStore matchStore, IPlayerActivity playerActivity)
    {
        ArgumentNullException.ThrowIfNull(sessionVerifier);
        ArgumentNullException.ThrowIfNull(matchStore);
        ArgumentNullException.ThrowIfNull(playerActivity);

        _sessionVerifier = sessionVerifier;
        _matchStore = matchStore;
        _playerActivity = playerActivity;
    }

    public ConnectedPlayer VerifySession(string sessionToken, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        return _sessionVerifier.Verify(sessionToken, now);
    }

    public ConnectedPlayer VerifyPlayerCanEnterRoom(string sessionToken, DateTime now)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        ConnectedPlayer player = VerifySession(sessionToken, now);
        if (!_playerActivity.HasNotificationChannel(player.AccountId))
        {
            throw new RoomRejectedException(RoomRejection.NotificationsUnavailable);
        }

        if (_playerActivity.IsInRoom(player.AccountId))
        {
            throw new RoomRejectedException(RoomRejection.InRoom);
        }

        if (_playerActivity.IsSearchingMatch(player.AccountId))
        {
            throw new RoomRejectedException(RoomRejection.SearchingMatch);
        }

        if (_matchStore.HasUnfinishedParticipation(player.AccountId))
        {
            throw new RoomRejectedException(RoomRejection.UnfinishedMatch);
        }

        return player;
    }
}
