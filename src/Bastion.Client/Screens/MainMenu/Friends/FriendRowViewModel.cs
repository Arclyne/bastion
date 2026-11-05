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

    public string Nickname
    {
        get => Get<string>();
        private set => Set(value);
    }

    public string FriendCode
    {
        get => Get<string>();
        private set => Set(value);
    }

    public bool IsConnected
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public string StatusText
    {
        get => Get<string>();
        private set => Set(value);
    }
}
