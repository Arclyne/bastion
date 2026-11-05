using System;
using Bastion.Client.Networking.Notifications;

namespace Bastion.Client.Tests.Fakes;

public sealed class FakeNotificationClient : INotificationClient
{
    public event EventHandler<ChatMessageReceivedEventArgs>? ChatMessageReceived;

    public event EventHandler<MatchChatMessageReceivedEventArgs>? MatchChatMessageReceived;

    public event EventHandler<MatchFoundEventArgs>? MatchFound;

    public event EventHandler<PresenceRequestedEventArgs>? PresenceRequested;

    public event EventHandler<SearchEndedEventArgs>? SearchEnded;

    public event EventHandler<FriendPresenceChangedEventArgs>? FriendPresenceChanged;

    public event EventHandler<FriendRequestReceivedEventArgs>? FriendRequestReceived;

    public event EventHandler<FriendshipStartedEventArgs>? FriendshipStarted;

    public event EventHandler<PrivateMessageReceivedEventArgs>? PrivateMessageReceived;

    public event EventHandler? Disconnected;

    public void Connect(string sessionToken)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
    }

    public void Disconnect()
    {
    }

    public bool HasAnySubscribers()
    {
        return ChatMessageReceived is not null
            || MatchChatMessageReceived is not null
            || MatchFound is not null
            || PresenceRequested is not null
            || SearchEnded is not null
            || FriendPresenceChanged is not null
            || FriendRequestReceived is not null
            || FriendshipStarted is not null
            || PrivateMessageReceived is not null
            || Disconnected is not null;
    }
}
