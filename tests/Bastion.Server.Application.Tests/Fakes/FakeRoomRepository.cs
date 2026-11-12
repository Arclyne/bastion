using System;
using System.Collections.Generic;
using Bastion.Domain.Matches;
using Bastion.Domain.Rooms;
using Bastion.Server.Application.Rooms;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeRoomRepository : IRoomRepository
{
    private const int FirstRoomId = 1;
    private const int FirstMatchId = 100;
    private const int NoOpenRooms = 0;

    private int _nextRoomId = FirstRoomId;
    private int _nextMatchId = FirstMatchId;

    public HashSet<string> TakenCodes { get; } = new HashSet<string>();

    public List<Room> AddedRooms { get; } = new List<Room>();

    public Dictionary<int, RoomSettings> ChangedSettingsByRoomId { get; } = new Dictionary<int, RoomSettings>();

    public List<int> ClosedRoomIds { get; } = new List<int>();

    public HashSet<string> CodesInMatch { get; } = new HashSet<string>();

    public Dictionary<int, Match> SavedMatchesByRoomId { get; } = new Dictionary<int, Match>();

    public RoomRepositoryException? MatchSaveFailure { get; set; }

    public bool IsCodeTaken(string code)
    {
        return TakenCodes.Contains(code);
    }

    public bool IsRoomInMatch(string code)
    {
        return CodesInMatch.Contains(code);
    }

    public int SaveRoomMatch(int roomId, Match match)
    {
        if (MatchSaveFailure is not null)
        {
            throw MatchSaveFailure;
        }

        SavedMatchesByRoomId[roomId] = match;
        int matchId = _nextMatchId;
        _nextMatchId++;
        return matchId;
    }

    public int AddRoom(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);

        AddedRooms.Add(room);
        int roomId = _nextRoomId;
        _nextRoomId++;
        return roomId;
    }

    public void ChangeRoomSettings(int roomId, RoomSettings settings, DateTime changedAt)
    {
        ChangedSettingsByRoomId[roomId] = settings;
    }

    public void CloseRoom(int roomId, DateTime closedAt)
    {
        ClosedRoomIds.Add(roomId);
    }

    public int CloseOpenRooms(DateTime closedAt)
    {
        return NoOpenRooms;
    }
}
