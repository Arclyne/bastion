using System;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Networking.Chat;
using Bastion.Client.Networking.Friends;
using Bastion.Client.Networking.Matchmaking;
using Bastion.Client.Networking.Notifications;

namespace Bastion.Client.Networking;

public sealed class ServerConnection : IDisposable
{
    private readonly AccountClient _accounts = new AccountClient();
    private readonly NotificationClient _notifications;
    private readonly ChatClient _chat;
    private readonly MatchmakingClient _matchmaking;
    private readonly FriendClient _friends = new FriendClient();

    public ServerConnection(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        _notifications = new NotificationClient(timeProvider);
        _chat = new ChatClient(_notifications);
        _matchmaking = new MatchmakingClient(_notifications);
        Accounts = _accounts;
        Notifications = _notifications;
        Chat = _chat;
        Matchmaking = _matchmaking;
        Friends = _friends;
    }

    public IAccountClient Accounts { get; }

    public INotificationClient Notifications { get; }

    public IChatClient Chat { get; }

    public IMatchmakingClient Matchmaking { get; }

    public IFriendClient Friends { get; }

    public void Dispose()
    {
        _notifications.Dispose();
        _chat.Dispose();
        _matchmaking.Dispose();
        _friends.Dispose();
        _accounts.Dispose();
    }
}
