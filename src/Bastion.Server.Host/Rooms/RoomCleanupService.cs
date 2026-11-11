using System;
using System.Threading;
using System.Threading.Tasks;
using Bastion.Server.Application.Rooms;
using Bastion.Server.Host.Sessions;
using log4net;
using Microsoft.Extensions.Hosting;

namespace Bastion.Server.Host.Rooms;

public sealed class RoomCleanupService : IHostedService
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(RoomCleanupService));

    private readonly NotificationChannelRegistry _channelRegistry;
    private readonly IRoomKeeper _roomKeeper;
    private readonly TimeProvider _timeProvider;

    public RoomCleanupService(
        NotificationChannelRegistry channelRegistry,
        IRoomKeeper roomKeeper,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(channelRegistry);
        ArgumentNullException.ThrowIfNull(roomKeeper);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _channelRegistry = channelRegistry;
        _roomKeeper = roomKeeper;
        _timeProvider = timeProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        CloseRoomsLeftOpen();
        _channelRegistry.PlayerDisconnected += HandlePlayerDisconnected;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _channelRegistry.PlayerDisconnected -= HandlePlayerDisconnected;
        return Task.CompletedTask;
    }

    private void CloseRoomsLeftOpen()
    {
        try
        {
            int closedRoomCount = _roomKeeper.CloseRoomsLeftOpen(GetUtcNow());
            _logger.Info($"Rooms left open by the last run were closed. ClosedRooms={closedRoomCount}");
        }
        catch (RoomRepositoryException ex)
        {
            _logger.Error("The rooms left open by the last run could not be closed.", ex);
        }
    }

    private void HandlePlayerDisconnected(object? sender, PlayerDisconnectedEventArgs e)
    {
        try
        {
            _roomKeeper.Leave(e.AccountId, GetUtcNow());
        }
        catch (RoomRepositoryException ex)
        {
            _logger.Error($"The room of a disconnected player could not be closed. AccountId={e.AccountId}", ex);
        }
    }

    private DateTime GetUtcNow()
    {
        return _timeProvider.GetUtcNow().UtcDateTime;
    }
}
