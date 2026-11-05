using System;
using System.Data.Common;
using System.Linq;
using Bastion.Domain.Rooms;
using Bastion.Server.Application.Rooms;
using Microsoft.EntityFrameworkCore;

namespace Bastion.Server.Adapters.Persistence;

public sealed class RoomStore : IRoomStore
{
    private readonly IDbContextFactory<BastionDbContext> _contextFactory;

    public RoomStore(IDbContextFactory<BastionDbContext> contextFactory)
    {
        ArgumentNullException.ThrowIfNull(contextFactory);

        _contextFactory = contextFactory;
    }

    public bool IsCodeTaken(string code)
    {
        ArgumentNullException.ThrowIfNull(code);

        return RunInNewContext(context => IsCodeTakenInContext(context, code));
    }

    public int AddRoom(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);

        return RunInNewContext(context => AddRoomInContext(context, room));
    }

    public void ChangeRoomSettings(int roomId, RoomSettings settings, DateTime changedAt)
    {
        ArgumentNullException.ThrowIfNull(settings);

        RunInNewContext(context => UpdateRoom(context, roomId, room => room.ChangeSettings(settings, changedAt)));
    }

    public void CloseRoom(int roomId, DateTime closedAt)
    {
        RunInNewContext(context => UpdateRoom(context, roomId, room => room.Close(closedAt)));
    }

    public int CloseOpenRooms(DateTime closedAt)
    {
        return RunInNewContext(context => CloseOpenRoomsInContext(context, closedAt));
    }

    private static bool IsCodeTakenInContext(BastionDbContext context, string code)
    {
        return context.Rooms.Any(room => room.Code == code && room.Status != RoomStatus.Closed);
    }

    private static int AddRoomInContext(BastionDbContext context, Room room)
    {
        context.Rooms.Add(room);
        context.SaveChanges();
        return room.RoomId;
    }

    private static int UpdateRoom(BastionDbContext context, int roomId, Action<Room> change)
    {
        Room? room = context.Rooms.FirstOrDefault(candidate => candidate.RoomId == roomId);
        if (room is null || room.Status != RoomStatus.Open)
        {
            return 0;
        }

        change(room);
        return context.SaveChanges();
    }

    private static int CloseOpenRoomsInContext(BastionDbContext context, DateTime closedAt)
    {
        return context.Rooms
            .Where(room => room.Status == RoomStatus.Open)
            .ExecuteUpdate(setters => setters
                .SetProperty(room => room.Status, RoomStatus.Closed)
                .SetProperty(room => room.LastActivityAt, closedAt));
    }

    private TResult RunInNewContext<TResult>(Func<BastionDbContext, TResult> operation)
    {
        try
        {
            using BastionDbContext context = _contextFactory.CreateDbContext();
            return operation(context);
        }
        catch (DbUpdateException ex)
        {
            throw new RoomStoreException(ex);
        }
        catch (DbException ex)
        {
            throw new RoomStoreException(ex);
        }
    }
}
