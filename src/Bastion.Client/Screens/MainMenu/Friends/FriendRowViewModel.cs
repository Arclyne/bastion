using System;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Friends;
using Gum.Mvvm;

namespace Bastion.Client.Screens.MainMenu.Friends;

public sealed class FriendRowViewModel : ViewModel
{
    public FriendRowViewModel(ListedFriend friend, DateTime listedAt)
    {
        ArgumentNullException.ThrowIfNull(friend);

        Nickname = friend.Friend.Nickname;
        FriendCode = friend.Friend.FriendCode;
        IsConnected = friend.Presence == FriendPresence.Connected;
        StatusText = IsConnected
            ? Strings.FriendsStatusInMenu
            : LastSeenText.DescribeLastSeen(friend.LastSeenAt, listedAt);
    }

    public string Nickname { get; }

    public string FriendCode { get; }

    public bool IsConnected { get; }

    public string StatusText { get; }
}
