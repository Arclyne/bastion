using System;
using System.Globalization;
using Bastion.Client.Localization;

namespace Bastion.Client.Screens.MainMenu.Friends;

public static class LastSeenText
{
    private const int JustNowMinutes = 1;
    private const int MinutesPerHour = 60;
    private const int HoursPerDay = 24;

    public static string DescribeLastSeen(DateTime? lastSeenAt, DateTime listedAt)
    {
        if (lastSeenAt is not DateTime lastSeen)
        {
            return Strings.FriendsStatusOffline;
        }

        TimeSpan elapsed = listedAt - lastSeen;
        if (elapsed.TotalMinutes < JustNowMinutes)
        {
            return Strings.FriendsLastSeenJustNow;
        }

        if (elapsed.TotalMinutes < MinutesPerHour)
        {
            return FormatAmount(Strings.FriendsLastSeenMinutesFormat, elapsed.TotalMinutes);
        }

        if (elapsed.TotalHours < HoursPerDay)
        {
            return FormatAmount(Strings.FriendsLastSeenHoursFormat, elapsed.TotalHours);
        }

        return FormatAmount(Strings.FriendsLastSeenDaysFormat, elapsed.TotalDays);
    }

    private static string FormatAmount(string format, double amount)
    {
        int wholeAmount = (int)amount;
        return string.Format(CultureInfo.CurrentCulture, format, wholeAmount);
    }
}
