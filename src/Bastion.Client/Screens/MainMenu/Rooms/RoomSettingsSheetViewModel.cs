using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Rooms;
using Bastion.Domain.Rooms;
using Gum.Mvvm;

namespace Bastion.Client.Screens.MainMenu.Rooms;

public sealed class RoomSettingsSheetViewModel : ViewModel
{
    public const byte DefaultClockMinutes = 5;

    private static readonly byte[] _clockMinuteOptions = { 3, 5, 10 };

    public RoomSettingsSheetViewModel()
    {
        IEnumerable<RoomModeOptionViewModel> modes = PrivateRoomModes.All.Select(
            mode => new RoomModeOptionViewModel(mode));
        Modes = [.. modes];
        IEnumerable<RoomClockOptionViewModel> clocks = _clockMinuteOptions.Select(
            minutes => new RoomClockOptionViewModel(minutes));
        Clocks = [.. clocks];
        ErrorText = string.Empty;
        ShowOptions(PrivateRoomGameMode.Classic, PrivateRoomModes.GetProposedWalls(PrivateRoomGameMode.Classic));
        SelectClock(DefaultClockMinutes);
    }

    public IReadOnlyList<RoomModeOptionViewModel> Modes { get; }

    public IReadOnlyList<RoomClockOptionViewModel> Clocks { get; }

    public bool IsOpen
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public bool IsEditing
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public PrivateRoomGameMode Mode
    {
        get => Get<PrivateRoomGameMode>();
        private set => Set(value);
    }

    public byte Walls
    {
        get => Get<byte>();
        private set => Set(value);
    }

    public byte ClockMinutes
    {
        get => Get<byte>();
        private set => Set(value);
    }

    public int MemberCount
    {
        get => Get<int>();
        private set => Set(value);
    }

    public bool IsSending
    {
        get => Get<bool>();
        set => Set(value);
    }

    public string ErrorText
    {
        get => Get<string>();
        private set => Set(value);
    }

    [DependsOn(nameof(ErrorText))]
    public bool HasError => ErrorText.Length > 0;

    [DependsOn(nameof(IsEditing))]
    public string TitleKey => IsEditing ? nameof(Strings.RoomEditTitle) : nameof(Strings.RoomCreateTitle);

    [DependsOn(nameof(IsEditing))]
    public string ConfirmKey => IsEditing ? nameof(Strings.RoomSave) : nameof(Strings.RoomOpen);

    [DependsOn(nameof(Walls))]
    public string WallsText => Walls.ToString(CultureInfo.CurrentCulture);

    [DependsOn(nameof(Mode))]
    public string WallsHintText => string.Format(
        CultureInfo.CurrentCulture,
        Strings.RoomWallsHintFormat,
        PrivateRoomModes.GetName(Mode),
        PrivateRoomModes.GetProposedWalls(Mode));

    [DependsOn(nameof(Walls))]
    [DependsOn(nameof(IsSending))]
    public bool CanDecreaseWalls => Walls > RoomPolicy.MinimumWallsPerPlayer && !IsSending;

    [DependsOn(nameof(Walls))]
    [DependsOn(nameof(IsSending))]
    public bool CanIncreaseWalls => Walls < RoomPolicy.MaximumWallsPerPlayer && !IsSending;

    [DependsOn(nameof(IsEditing))]
    [DependsOn(nameof(MemberCount))]
    public bool HasMembersNotice => IsEditing && MemberCount > 1;

    [DependsOn(nameof(IsSending))]
    public bool CanConfirm => !IsSending;

    public void OpenToCreate()
    {
        IsEditing = false;
        MemberCount = 1;
        ShowMemberCount();
        ShowOptions(PrivateRoomGameMode.Classic, PrivateRoomModes.GetProposedWalls(PrivateRoomGameMode.Classic));
        SelectClock(DefaultClockMinutes);
        Open();
    }

    public void OpenToEdit(PrivateRoomOptions options, int memberCount)
    {
        ArgumentNullException.ThrowIfNull(options);

        IsEditing = true;
        MemberCount = memberCount;
        ShowMemberCount();
        ShowOptions(options.GameMode, options.WallsPerPlayer);
        SelectClock(options.ClockMinutes);
        Open();
    }

    public void SelectMode(PrivateRoomGameMode mode)
    {
        RoomModeOptionViewModel option = Modes.First(candidate => candidate.Mode == mode);
        if (!option.IsAvailable || IsSending)
        {
            return;
        }

        ShowOptions(mode, PrivateRoomModes.GetProposedWalls(mode));
    }

    public void DecreaseWalls()
    {
        if (CanDecreaseWalls)
        {
            Walls--;
        }
    }

    public void IncreaseWalls()
    {
        if (CanIncreaseWalls)
        {
            Walls++;
        }
    }

    public void SelectClock(byte minutes)
    {
        ClockMinutes = minutes;
        foreach (RoomClockOptionViewModel clock in Clocks)
        {
            clock.IsSelected = clock.Minutes == minutes;
        }
    }

    public PrivateRoomOptions GetOptions()
    {
        return new PrivateRoomOptions(Mode, Walls, ClockMinutes);
    }

    public void ShowError(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        ErrorText = text;
    }

    public void Close()
    {
        IsOpen = false;
        IsSending = false;
    }

    private void ShowOptions(PrivateRoomGameMode mode, byte walls)
    {
        Mode = mode;
        Walls = walls;
        foreach (RoomModeOptionViewModel option in Modes)
        {
            option.IsSelected = option.Mode == mode;
        }
    }

    private void ShowMemberCount()
    {
        foreach (RoomModeOptionViewModel option in Modes)
        {
            option.ShowMemberCount(MemberCount);
        }
    }

    private void Open()
    {
        ErrorText = string.Empty;
        IsSending = false;
        IsOpen = true;
    }
}
