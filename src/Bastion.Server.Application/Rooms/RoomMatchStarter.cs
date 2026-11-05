using System;
using Bastion.Server.Application.Matchmaking;

namespace Bastion.Server.Application.Rooms;

public sealed class RoomMatchStarter : IRoomMatchStarter
{
    private readonly IRoomRegistry _roomRegistry;
    private readonly IPrivateMatchCreator _matchCreator;
    private readonly IMatchmakingNotifier _matchmakingNotifier;

    public RoomMatchStarter(
        IRoomRegistry roomRegistry,
        IPrivateMatchCreator matchCreator,
        IMatchmakingNotifier matchmakingNotifier)
    {
        ArgumentNullException.ThrowIfNull(roomRegistry);
        ArgumentNullException.ThrowIfNull(matchCreator);
        ArgumentNullException.ThrowIfNull(matchmakingNotifier);

        _roomRegistry = roomRegistry;
        _matchCreator = matchCreator;
        _matchmakingNotifier = matchmakingNotifier;
    }

    public CreatedMatch StartMatch(int hostAccountId, DateTime now)
    {
        CreatedMatch createdMatch =
            _roomRegistry.StartMatch(hostAccountId, room => _matchCreator.CreateMatch(room, now));
        _matchmakingNotifier.NotifyMatchFound(createdMatch);
        return createdMatch;
    }
}
