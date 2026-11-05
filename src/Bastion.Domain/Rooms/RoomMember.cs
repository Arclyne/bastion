using System;

namespace Bastion.Domain.Rooms;

public sealed class RoomMember
{
    public RoomMember(int accountId, string nickname)
    {
        ArgumentNullException.ThrowIfNull(nickname);

        AccountId = accountId;
        Nickname = nickname;
    }

    public int AccountId { get; }

    public string Nickname { get; }

    public int SeatIndex { get; private set; }

    public bool IsReady { get; private set; }

    internal void TakeSeat(int seatIndex)
    {
        SeatIndex = seatIndex;
    }

    internal void MarkReady(bool isReady)
    {
        IsReady = isReady;
    }
}
