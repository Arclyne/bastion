using Bastion.Client.Networking.Chat;
using Bastion.Client.Networking.Friends;
using Bastion.Client.Networking.Notifications;

namespace Bastion.Client.Screens.MainMenu.Friends;

public sealed record SocialClients(IFriendClient Friends, IChatClient Chat, INotificationClient Notifications);
