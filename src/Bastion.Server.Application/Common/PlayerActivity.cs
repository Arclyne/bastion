using System;
using Bastion.Server.Application.Matchmaking;
using Bastion.Server.Application.Rooms;

namespace Bastion.Server.Application.Common;

public sealed class PlayerActivity : IPlayerActivity
{
    private readonly IMatchmakingNotifier _matchmakingNotifier;
    private readonly IMatchmakingQueue _matchmakingQueue;
    private readonly IRoomRegistry _roomRegistry;

    public PlayerActivity(
        IMatchmakingNotifier matchmakingNotifier,
        IMatchmakingQueue matchmakingQueue,
        IRoomRegistry roomRegistry)
    {
        ArgumentNullException.ThrowIfNull(matchmakingNotifier);
        ArgumentNullException.ThrowIfNull(matchmakingQueue);
        ArgumentNullException.ThrowIfNull(roomRegistry);

        _matchmakingNotifier = matchmakingNotifier;
        _matchmakingQueue = matchmakingQueue;
        _roomRegistry = roomRegistry;
    }

    public bool HasNotificationChannel(int accountId)
    {
        return _matchmakingNotifier.HasNotificationChannel(accountId);
    }

    public bool IsSearchingMatch(int accountId)
    {
        return _matchmakingQueue.IsSearching(accountId);
    }

    public bool IsInRoom(int accountId)
    {
        return _roomRegistry.IsInRoom(accountId);
    }
}
