using System;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Chat;
using Bastion.Client.Networking.Friends;
using Bastion.Client.Networking.Matches;
using Bastion.Client.Networking.Matchmaking;
using Bastion.Client.Networking.Notifications;
using Bastion.Client.Networking.PrivateChat;
using Bastion.Client.Networking.Rooms;
using Bastion.Contracts;
using Bastion.Contracts.Accounts;
using Bastion.Contracts.Chat;
using Bastion.Contracts.Friends;
using Bastion.Contracts.Matches;
using Bastion.Contracts.Matchmaking;
using Bastion.Contracts.PrivateChat;
using Bastion.Contracts.Rooms;

namespace Bastion.Client.Networking;

public sealed class ServerConnection : IDisposable
{
    private readonly ServiceChannel<IAccountService> _accountChannel;
    private readonly ServiceChannel<IChatService> _chatChannel;
    private readonly ServiceChannel<IMatchmakingService> _matchmakingChannel;
    private readonly ServiceChannel<IMatchService> _matchChannel;
    private readonly ServiceChannel<IFriendshipService> _friendshipChannel;
    private readonly ServiceChannel<IConversationService> _conversationChannel;
    private readonly ServiceChannel<IRoomService> _roomChannel;
    private readonly SessionChannelFactory _sessionChannelFactory;
    private readonly NotificationClient _notifications;

    public ServerConnection(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        _accountChannel = new ServiceChannel<IAccountService>(ServiceEndpoints.AccountsPath);
        _chatChannel = new ServiceChannel<IChatService>(ServiceEndpoints.ChatPath);
        _matchmakingChannel = new ServiceChannel<IMatchmakingService>(ServiceEndpoints.MatchmakingPath);
        _matchChannel = new ServiceChannel<IMatchService>(ServiceEndpoints.MatchesPath);
        _friendshipChannel = new ServiceChannel<IFriendshipService>(ServiceEndpoints.FriendsPath);
        _conversationChannel = new ServiceChannel<IConversationService>(ServiceEndpoints.ConversationsPath);
        _roomChannel = new ServiceChannel<IRoomService>(ServiceEndpoints.RoomsPath);
        var notificationCallback = new NotificationCallback();
        _sessionChannelFactory = new SessionChannelFactory(notificationCallback);
        _notifications = new NotificationClient(notificationCallback, _sessionChannelFactory, timeProvider);
        Accounts = new AccountClient(_accountChannel);
        Notifications = _notifications;
        Chat = new ChatClient(_chatChannel, _notifications);
        Matchmaking = new MatchmakingClient(_matchmakingChannel, _notifications);
        Matches = new MatchClient(_matchChannel, _notifications);
        Friends = new FriendClient(_friendshipChannel);
        Conversations = new ConversationClient(_conversationChannel);
        Rooms = new RoomClient(_roomChannel, _notifications);
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
        _sessionChannelFactory.Dispose();
        _chatChannel.Dispose();
        _matchmakingChannel.Dispose();
        _matchChannel.Dispose();
        _friendshipChannel.Dispose();
        _conversationChannel.Dispose();
        _roomChannel.Dispose();
        _accountChannel.Dispose();
    }
}
