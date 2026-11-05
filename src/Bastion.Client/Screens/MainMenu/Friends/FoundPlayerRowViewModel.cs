using System;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Friends;
using Gum.Mvvm;

namespace Bastion.Client.Screens.MainMenu.Friends;

public sealed class FoundPlayerRowViewModel : ViewModel
{
    public FoundPlayerRowViewModel(FoundPlayer player)
    {
        ArgumentNullException.ThrowIfNull(player);

        Nickname = player.Player.Nickname;
        FriendCode = player.Player.FriendCode;
        Relationship = player.Relationship;
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

    public FriendRelationship Relationship
    {
        get => Get<FriendRelationship>();
        private set => Set(value);
    }

    public bool IsSending
    {
        get => Get<bool>();
        set => Set(value);
    }

    [DependsOn(nameof(Relationship))]
    public string ActionKey => Relationship switch
    {
        FriendRelationship.None => nameof(Strings.FriendsAdd),
        FriendRelationship.RequestReceived => nameof(Strings.FriendsAccept),
        FriendRelationship.RequestSent => nameof(Strings.FriendsRequestSent),
        FriendRelationship.AlreadyFriends => nameof(Strings.FriendsAlreadyFriend),
        _ => nameof(Strings.FriendsItsYou),
    };

    [DependsOn(nameof(Relationship))]
    [DependsOn(nameof(IsSending))]
    public bool CanPress =>
        !IsSending && (Relationship == FriendRelationship.None || Relationship == FriendRelationship.RequestReceived);

    public void ApplyOutcome(FriendRequestOutcome outcome)
    {
        Relationship = outcome == FriendRequestOutcome.FriendshipStarted
            ? FriendRelationship.AlreadyFriends
            : FriendRelationship.RequestSent;
    }

    public void MarkRequestSent()
    {
        Relationship = FriendRelationship.RequestSent;
    }
}
