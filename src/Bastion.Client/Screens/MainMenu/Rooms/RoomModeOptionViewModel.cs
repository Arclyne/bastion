using System.Globalization;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Rooms;
using Gum.Mvvm;

namespace Bastion.Client.Screens.MainMenu.Rooms;

public sealed class RoomModeOptionViewModel : ViewModel
{
    public RoomModeOptionViewModel(PrivateRoomGameMode mode)
    {
        Mode = mode;
        NameText = PrivateRoomModes.GetName(mode);
        int seatCount = PrivateRoomModes.GetSeatCount(mode);
        int boardSize = PrivateRoomModes.GetBoardSize(mode);
        DetailText = string.Format(CultureInfo.CurrentCulture, Strings.RoomModeDetailFormat, seatCount, boardSize);
        CaptionText = string.Empty;
        IsAvailable = true;
        ShowMemberCount(memberCount: 1);
    }

    public PrivateRoomGameMode Mode { get; }

    public string NameText { get; }

    public string DetailText { get; }

    public string CaptionText
    {
        get => Get<string>();
        private set => Set(value);
    }

    public bool IsSelected
    {
        get => Get<bool>();
        set => Set(value);
    }

    public bool IsAvailable
    {
        get => Get<bool>();
        private set => Set(value);
    }

    [DependsOn(nameof(IsSelected))]
    [DependsOn(nameof(IsAvailable))]
    public bool IsIdle => IsAvailable && !IsSelected;

    [DependsOn(nameof(IsAvailable))]
    public bool IsCrowded => !IsAvailable;

    public void ShowMemberCount(int memberCount)
    {
        IsAvailable = memberCount <= PrivateRoomModes.GetSeatCount(Mode);
        if (IsAvailable)
        {
            byte walls = PrivateRoomModes.GetProposedWalls(Mode);
            CaptionText = string.Format(CultureInfo.CurrentCulture, Strings.RoomModeWallsFormat, walls);
            return;
        }

        CaptionText = string.Format(CultureInfo.CurrentCulture, Strings.RoomModeCrowdedFormat, memberCount);
    }
}
