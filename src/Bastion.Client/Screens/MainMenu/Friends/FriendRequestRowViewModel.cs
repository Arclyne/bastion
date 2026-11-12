using System;
using System.Globalization;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Friends;
using Gum.Mvvm;

namespace Bastion.Client.Screens.MainMenu.Friends;

public sealed class FriendRequestRowViewModel : ViewModel
{
    private const int LastDay = 1;

    public FriendRequestRowViewModel(PendingFriendRequest request, DateTime listedAt, bool isReceived)
    {
        ArgumentNullException.ThrowIfNull(request);

        Nickname = request.Player.Nickname;
        FriendCode = request.Player.FriendCode;
        IsReceived = isReceived;
        DetailText = DescribeExpiry(request.ExpiresAt, listedAt);
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

    public string DetailText
    {
        get => Get<string>();
        private set => Set(value);
    }

    public bool IsReceived
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public bool IsBusy
    {
        get => Get<bool>();
        set => Set(value);
    }

    [DependsOn(nameof(IsBusy))]
    public bool CanAct => !IsBusy;

    private static string DescribeExpiry(DateTime expiresAt, DateTime listedAt)
    {
        TimeSpan remaining = expiresAt - listedAt;
        double roundedUpDays = Math.Ceiling(remaining.TotalDays);
        int daysLeft = (int)roundedUpDays;
        if (daysLeft <= LastDay)
        {
            return Strings.FriendsExpiresTomorrow;
        }

        return string.Format(CultureInfo.CurrentCulture, Strings.FriendsExpiresInDaysFormat, daysLeft);
    }
}
