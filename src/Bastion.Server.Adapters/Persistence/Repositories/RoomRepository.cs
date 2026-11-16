using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Matches;
using Bastion.Domain.Rooms;
using Bastion.Server.Application.Rooms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Bastion.Server.Adapters.Persistence.Repositories;

public sealed class RoomRepository : IRoomRepository
{
    private readonly NewContextRunner<RoomRepositoryException> _newContextRunner;

    public RoomRepository(IDbContextFactory<BastionDbContext> contextFactory)
    {
        ArgumentNullException.ThrowIfNull(contextFactory);

        _newContextRunner = new NewContextRunner<RoomRepositoryException>(
            contextFactory,
            innerException => new RoomRepositoryException(innerException));
    }

    public bool IsCodeTaken(string code)
    {
        ArgumentNullException.ThrowIfNull(code);

        Func<BastionDbContext, bool> isCodeTaken = context => IsCodeTakenInContext(context, code);
        return _newContextRunner.Run(isCodeTaken);
    }

    public bool IsRoomInMatch(string code)
    {
        ArgumentNullException.ThrowIfNull(code);

        Func<BastionDbContext, bool> isRoomInMatch = context => IsRoomInMatchInContext(context, code);
        return _newContextRunner.Run(isRoomInMatch);
    }

    public int AddRoom(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);

        Func<BastionDbContext, int> addRoom = context => AddRoomInContext(context, room);
        return _newContextRunner.Run(addRoom);
    }

    public void ChangeRoomSettings(int roomId, RoomSettings settings, DateTime changedAt)
    {
        ArgumentNullException.ThrowIfNull(settings);

        Action<Room> changeSettings = room => room.ChangeSettings(settings, changedAt);
        Func<BastionDbContext, int> updateOpenRoom = context => UpdateOpenRoom(context, roomId, changeSettings);
        _newContextRunner.Run(updateOpenRoom);
    }

    public void CloseRoom(int roomId, DateTime closedAt)
    {
        Action<Room> closeRoom = room => room.Close(closedAt);
        Func<BastionDbContext, int> updateOpenRoom = context => UpdateOpenRoom(context, roomId, closeRoom);
        _newContextRunner.Run(updateOpenRoom);
    }

    public int CloseOpenRooms(DateTime closedAt)
    {
        Func<BastionDbContext, int> closeOpenRooms = context => CloseOpenRoomsInContext(context, closedAt);
        return _newContextRunner.Run(closeOpenRooms);
    }

    public int SaveRoomMatch(int roomId, Match match)
    {
        ArgumentNullException.ThrowIfNull(match);

        Func<BastionDbContext, int> saveRoomMatch = context => AddMatchAndStartRoom(context, roomId, match);
        return _newContextRunner.Run(saveRoomMatch);
    }

    private static bool IsCodeTakenInContext(BastionDbContext context, string code)
    {
        IQueryable<Room> roomsNotClosed = SelectRoomsWithCode(context, code)
            .Where(room => room.Status != RoomStatus.Closed);
        return roomsNotClosed.Any();
    }

    private static IQueryable<Room> SelectRoomsWithCode(BastionDbContext context, string code)
    {
        IQueryable<Room> roomsWithCode = context.Rooms
            .AsNoTracking()
            .Where(room => room.Code == code);
        return roomsWithCode;
    }

    private static bool IsRoomInMatchInContext(BastionDbContext context, string code)
    {
        IQueryable<Room> roomsInMatch = SelectRoomsWithCode(context, code)
            .Where(room => room.Status == RoomStatus.InMatch);
        return roomsInMatch.Any();
    }

    private static int AddRoomInContext(BastionDbContext context, Room room)
    {
        context.Rooms.Add(room);
        context.SaveChanges();
        return room.RoomId;
    }

    private static int UpdateOpenRoom(BastionDbContext context, int roomId, Action<Room> change)
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
        IQueryable<Room> openRooms = context.Rooms
            .Where(room => room.Status == RoomStatus.Open);
        var roomsLeftOpen = new List<Room>(openRooms);
        foreach (Room room in roomsLeftOpen)
        {
            room.Close(closedAt);
        }

        return context.SaveChanges();
    }

    private static int AddMatchAndStartRoom(BastionDbContext context, int roomId, Match match)
    {
        using IDbContextTransaction transaction = context.Database.BeginTransaction();
        context.Matches.Add(match);
        context.SaveChanges();
        Room room = context.Rooms.First(candidate => candidate.RoomId == roomId);
        room.StartMatch(match.MatchId, match.StartedAt);
        context.SaveChanges();
        transaction.Commit();
        return match.MatchId;
    }
}
