using System;

namespace Bastion.Domain.Rooms;

public class Room
{
    private Room(RoomOpening opening, RoomSettings settings)
    {
        Code = opening.Code;
        HostId = opening.HostId;
        Status = RoomStatus.Open;
        CreatedAt = opening.OpenedAt;
        ApplySettings(settings, opening.OpenedAt);
    }

    private Room()
    {
        Code = string.Empty;
    }

    public int RoomId { get; private set; }

    public string Code { get; private set; }

    public int HostId { get; private set; }

    public byte GameModeId { get; private set; }

    public byte WallsPerPlayer { get; private set; }

    public byte ClockMinutes { get; private set; }

    public RoomStatus Status { get; private set; }

    public int? MatchId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime LastActivityAt { get; private set; }

    public static Room Open(RoomOpening opening, RoomSettings settings)
    {
        ArgumentNullException.ThrowIfNull(opening);
        ArgumentNullException.ThrowIfNull(settings);
        if (!RoomRules.IsValidCode(opening.Code))
        {
            throw new ArgumentException("The room code does not follow the code rules.", nameof(opening));
        }

        return new Room(opening, settings);
    }

    public void ChangeSettings(RoomSettings settings, DateTime changedAt)
    {
        ArgumentNullException.ThrowIfNull(settings);
        EnsureOpen();

        ApplySettings(settings, changedAt);
    }

    public void Close(DateTime closedAt)
    {
        EnsureOpen();

        Status = RoomStatus.Closed;
        LastActivityAt = closedAt;
    }

    public void StartMatch(int matchId, DateTime startedAt)
    {
        EnsureOpen();

        Status = RoomStatus.InMatch;
        MatchId = matchId;
        LastActivityAt = startedAt;
    }

    public void FinishMatch(DateTime finishedAt)
    {
        if (Status != RoomStatus.InMatch)
        {
            throw new InvalidOperationException("Only a room in a match can finish it.");
        }

        Status = RoomStatus.Closed;
        LastActivityAt = finishedAt;
    }

    private void ApplySettings(RoomSettings settings, DateTime changedAt)
    {
        if (!settings.IsValid())
        {
            throw new ArgumentException("The room settings are not valid.", nameof(settings));
        }

        GameModeId = settings.GameMode.GameModeId;
        WallsPerPlayer = settings.WallsPerPlayer;
        ClockMinutes = settings.ClockMinutes;
        LastActivityAt = changedAt;
    }

    private void EnsureOpen()
    {
        if (Status != RoomStatus.Open)
        {
            throw new InvalidOperationException("Only an open room can change.");
        }
    }
}
