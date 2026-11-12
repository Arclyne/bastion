using System;
using System.Globalization;
using Bastion.Client.Localization;

namespace Bastion.Client.Screens.MainMenu.Chat;

public static class ChatTimeText
{
    private const string TimeFormat = "HH:mm";
    private const string DayFormat = "d MMM";
    private const string MomentFormat = "g";
    private const int OneDay = 1;

    public static string FormatTime(DateTime sentAt)
    {
        DateTime localTime = sentAt.ToLocalTime();
        return localTime.ToString(TimeFormat, CultureInfo.CurrentCulture);
    }

    public static string FormatMoment(DateTime moment)
    {
        DateTime localMoment = moment.ToLocalTime();
        return localMoment.ToString(MomentFormat, CultureInfo.CurrentCulture);
    }

    public static DateTime GetLocalToday(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        DateTimeOffset now = timeProvider.GetLocalNow();
        return now.Date;
    }

    public static DateTime GetLocalDay(DateTime moment)
    {
        DateTime localMoment = moment.ToLocalTime();
        return localMoment.Date;
    }

    public static string DescribeDay(DateTime sentAt, DateTime today)
    {
        DateTime day = GetLocalDay(sentAt);
        if (day == today)
        {
            return Strings.ChatToday;
        }

        if (day == today.AddDays(-OneDay))
        {
            return Strings.ChatYesterday;
        }

        return day.ToString(DayFormat, CultureInfo.CurrentCulture);
    }

    public static string DescribeLastActivity(DateTime sentAt, DateTime today)
    {
        DateTime day = GetLocalDay(sentAt);
        if (day == today)
        {
            return FormatTime(sentAt);
        }

        return DescribeDay(sentAt, today);
    }
}
