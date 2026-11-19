using System;
using System.Globalization;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Rooms;
using Gum.Mvvm;

namespace Bastion.Client.Screens.MainMenu.Rooms;

public sealed class RoomSeatViewModel : ViewModel
{
    public RoomSeatViewModel(int seatIndex)
    {
        SeatIndex = seatIndex;
        Nickname = string.Empty;
        NameText = string.Empty;
    }

    public int SeatIndex { get; }

    public string Nickname
    {
        get => Get<string>();
        private set => Set(value);
    }

    public string NameText
    {
        get => Get<string>();
        private set => Set(value);
    }

    public bool IsTaken
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public bool IsHost
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public bool IsReady
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public bool IsKickable
    {
        get => Get<bool>();
        private set => Set(value);
    }

    [DependsOn(nameof(IsTaken))]
    public bool IsFree => !IsTaken;

    [DependsOn(nameof(IsTaken))]
    [DependsOn(nameof(IsReady))]
    public bool IsNotReady => IsTaken && !IsReady;

    public void ShowMember(PrivateRoomMember member, string ownNickname, bool isHostView)
    {
        ArgumentNullException.ThrowIfNull(member);
        ArgumentNullException.ThrowIfNull(ownNickname);

        bool isOwn = member.Nickname == ownNickname;
        Nickname = member.Nickname;
        NameText = isOwn
            ? string.Format(CultureInfo.CurrentCulture, Strings.RoomOwnNameFormat, member.Nickname)
            : member.Nickname;
        IsHost = member.IsHost;
        IsReady = member.IsReady;
        IsKickable = isHostView && !isOwn;
        IsTaken = true;
    }

    public void ShowFree()
    {
        Nickname = string.Empty;
        NameText = string.Empty;
        IsHost = false;
        IsReady = false;
        IsKickable = false;
        IsTaken = false;
    }
}
