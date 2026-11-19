using System.Globalization;
using Bastion.Client.Localization;
using Gum.Mvvm;

namespace Bastion.Client.Screens.MainMenu.Rooms;

public sealed class RoomClockOptionViewModel : ViewModel
{
    public RoomClockOptionViewModel(byte minutes)
    {
        Minutes = minutes;
        Text = string.Format(CultureInfo.CurrentCulture, Strings.RoomClockFormat, minutes);
    }

    public byte Minutes { get; }

    public string Text { get; }

    public bool IsSelected
    {
        get => Get<bool>();
        set => Set(value);
    }

    [DependsOn(nameof(IsSelected))]
    public bool IsIdle => !IsSelected;
}
