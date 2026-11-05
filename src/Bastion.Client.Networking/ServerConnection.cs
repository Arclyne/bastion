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
    private readonly ChatClient _chat = new ChatClient();
    private readonly MatchmakingClient _matchmaking = new MatchmakingClient();
    private readonly FriendClient _friends = new FriendClient();

    public ServerConnection(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        _notifications = new NotificationClient(timeProvider);
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
