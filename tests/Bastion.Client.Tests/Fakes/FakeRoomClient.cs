using System;
using System.Collections.Generic;
using Bastion.Client.Networking.Notifications;
using Bastion.Client.Networking.Rooms;

namespace Bastion.Client.Tests.Fakes;

public sealed class FakeRoomClient : IRoomClient
{
    public FakeRoomClient()
    {
        KickedNicknames = [];
        OwnRoom = OwnRoom.CreateOutsideRoom();
    }

    public event EventHandler<RoomUpdatedEventArgs>? RoomUpdated;

    public event EventHandler<RoomClosedEventArgs>? RoomClosed;

    public event EventHandler? Disconnected;

    public PrivateRoom? NextRoom { get; set; }

    public Exception? Failure { get; set; }

    public OwnRoom OwnRoom { get; set; }

    public PrivateRoomOptions? CreatedOptions { get; private set; }

    public PrivateRoomOptions? ChangedOptions { get; private set; }

    public string JoinedCode { get; private set; } = string.Empty;

    public List<string> KickedNicknames { get; }

    public int StartCount { get; private set; }

    public int LeaveCount { get; private set; }

    public PrivateRoom CreateRoom(string sessionToken, PrivateRoomOptions options)
    {
        CreatedOptions = options;
        return AnswerRoom();
    }

    public PrivateRoom JoinRoom(string sessionToken, string code)
    {
        JoinedCode = code;
        return AnswerRoom();
    }

    public PrivateRoom SetReady(string sessionToken, bool isReady)
    {
        return AnswerRoom();
    }

    public PrivateRoom ChangeOptions(string sessionToken, PrivateRoomOptions options)
    {
        ChangedOptions = options;
        return AnswerRoom();
    }

    public void StartMatch(string sessionToken)
    {
        ThrowFailure();
        StartCount++;
    }

    public PrivateRoom KickPlayer(string sessionToken, string nickname)
    {
        KickedNicknames.Add(nickname);
        return AnswerRoom();
    }

    public void LeaveRoom(string sessionToken)
    {
        LeaveCount++;
    }

    public OwnRoom FindOwnRoom(string sessionToken)
    {
        ThrowFailure();
        return OwnRoom;
    }

    public void RaiseRoomUpdated(PrivateRoom room)
    {
        var updated = new RoomUpdatedEventArgs(room);
        RoomUpdated?.Invoke(this, updated);
    }

    public void RaiseRoomClosed(string code, RoomCloseReason reason)
    {
        var closed = new RoomClosedEventArgs(code, reason);
        RoomClosed?.Invoke(this, closed);
    }

    public void RaiseDisconnected()
    {
        Disconnected?.Invoke(this, EventArgs.Empty);
    }

    private PrivateRoom AnswerRoom()
    {
        ThrowFailure();
        return NextRoom ?? throw new InvalidOperationException("The fake has no room to answer with.");
    }

    private void ThrowFailure()
    {
        if (Failure is not null)
        {
            throw Failure;
        }
    }
}
