using System;
using System.Collections.Generic;
using Bastion.Domain.Matches;
using Bastion.Domain.Rooms;
using Bastion.Server.Application.Rooms;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeRoomRepository : IRoomRepository
{
    private const int FirstRoomId = 1;
    public const int FirstMatchId = 100;

    private int _nextRoomId;
    private int _nextMatchId;

    public FakeRoomRepository()
    {
        _nextRoomId = FirstRoomId;
        _nextMatchId = FirstMatchId;
        TakenCodes = [];
        AddedRooms = [];
        ChangedSettingsByRoomId = [];
        ClosedRoomIds = [];
        CodesInMatch = [];
        SavedMatchesByRoomId = [];
    }

    public HashSet<string> TakenCodes { get; }

    public List<Room> AddedRooms { get; }

    public Dictionary<int, RoomSettings> ChangedSettingsByRoomId { get; }

    public List<int> ClosedRoomIds { get; }

    public HashSet<string> CodesInMatch { get; }

    public Dictionary<int, Match> SavedMatchesByRoomId { get; }

    public RoomRepositoryException? MatchSaveFailure { get; set; }

    public RoomRepositoryException? InMatchCheckFailure { get; set; }

    public bool IsCodeTaken(string code)
    {
        return TakenCodes.Contains(code);
    }

    public bool IsRoomInMatch(string code)
    {
        if (InMatchCheckFailure is not null)
        {
            throw InMatchCheckFailure;
        }

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
        return 0;
    }
}
