using System;
using System.Globalization;
using Bastion.Client.Localization;

namespace Bastion.Client.Screens.MainMenu.Friends;

public static class LastSeenText
{
    private const int MinutesPerHour = 60;
    private const int HoursPerDay = 24;

    public static string DescribeLastSeen(DateTime? lastSeenAt, DateTime listedAt)
    {
        if (lastSeenAt is null)
        {
            return Strings.FriendsStatusOffline;
        }

        TimeSpan elapsed = listedAt - lastSeenAt.Value;
        if (elapsed.TotalMinutes < 1)
        {
            return Strings.FriendsLastSeenJustNow;
        }

        if (elapsed.TotalMinutes < MinutesPerHour)
        {
            return FormatAmount(Strings.FriendsLastSeenMinutesFormat, (int)elapsed.TotalMinutes);
        }

        if (elapsed.TotalHours < HoursPerDay)
        {
            return FormatAmount(Strings.FriendsLastSeenHoursFormat, (int)elapsed.TotalHours);
        }

        return FormatAmount(Strings.FriendsLastSeenDaysFormat, (int)elapsed.TotalDays);
    }

    private static string FormatAmount(string format, int amount)
    {
        return string.Format(CultureInfo.CurrentCulture, format, amount);
    }
}
