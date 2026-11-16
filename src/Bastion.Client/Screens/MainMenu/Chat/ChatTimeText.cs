using System;
using System.Globalization;
using Bastion.Client.Localization;

namespace Bastion.Client.Screens.MainMenu.Chat;

public static class ChatTimeText
{
    private const string TimeFormat = "HH:mm";
    private const string DayFormat = "d MMM";
    private const string MomentFormat = "g";

    public static string FormatTime(DateTime sentAt, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        DateTime localTime = ToLocalTime(sentAt, timeProvider);
        return localTime.ToString(TimeFormat, CultureInfo.CurrentCulture);
    }

    public static string FormatMoment(DateTime moment, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        DateTime localMoment = ToLocalTime(moment, timeProvider);
        return localMoment.ToString(MomentFormat, CultureInfo.CurrentCulture);
    }

    public static DateTime GetLocalToday(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        DateTimeOffset now = timeProvider.GetLocalNow();
        return now.Date;
    }

    public static DateTime GetLocalDay(DateTime moment, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        DateTime localMoment = ToLocalTime(moment, timeProvider);
        return localMoment.Date;
    }

    public static string DescribeDay(DateTime sentAt, DateTime today, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        DateTime day = GetLocalDay(sentAt, timeProvider);
        if (day == today)
        {
            return Strings.ChatToday;
        }

        if (day == today.AddDays(-1))
        {
            return Strings.ChatYesterday;
        }

        return day.ToString(DayFormat, CultureInfo.CurrentCulture);
    }

    public static string DescribeLastActivity(DateTime sentAt, DateTime today, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        DateTime day = GetLocalDay(sentAt, timeProvider);
        if (day == today)
        {
            return FormatTime(sentAt, timeProvider);
        }

        return DescribeDay(sentAt, today, timeProvider);
    }

    private static DateTime ToLocalTime(DateTime moment, TimeProvider timeProvider)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(moment, timeProvider.LocalTimeZone);
    }
}
