using System;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Networking.Chat;
using Bastion.Client.Networking.Friends;
using Bastion.Client.Networking.Matches;
using Bastion.Client.Networking.Matchmaking;
using Bastion.Client.Networking.Notifications;
using Bastion.Client.Networking.PrivateChat;
using Bastion.Client.Networking.Rooms;

namespace Bastion.Client.Networking;

public sealed class ServerConnection : IDisposable
{
    private readonly AccountClient _accounts = new AccountClient();
    private readonly NotificationClient _notifications;
    private readonly ChatClient _chat;
    private readonly MatchmakingClient _matchmaking;
    private readonly MatchClient _matches;
    private readonly FriendClient _friends = new FriendClient();
    private readonly ConversationClient _conversations = new ConversationClient();
    private readonly RoomClient _rooms;

    public ServerConnection(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        _notifications = new NotificationClient(timeProvider);
        _chat = new ChatClient(_notifications);
        _matchmaking = new MatchmakingClient(_notifications);
        _matches = new MatchClient(_notifications);
        _rooms = new RoomClient(_notifications);
        Accounts = _accounts;
        Notifications = _notifications;
        Chat = _chat;
        Matchmaking = _matchmaking;
        Matches = _matches;
        Friends = _friends;
        Conversations = _conversations;
        Rooms = _rooms;
    }

    public IAccountClient Accounts { get; }

    public INotificationClient Notifications { get; }

    public IChatClient Chat { get; }

    public IMatchmakingClient Matchmaking { get; }

    public IMatchClient Matches { get; }

    public IFriendClient Friends { get; }

    public IConversationClient Conversations { get; }

    public IRoomClient Rooms { get; }

    public void Dispose()
    {
        _notifications.Dispose();
        _chat.Dispose();
        _matchmaking.Dispose();
        _matches.Dispose();
        _friends.Dispose();
        _conversations.Dispose();
        _rooms.Dispose();
        _accounts.Dispose();
    }
}
