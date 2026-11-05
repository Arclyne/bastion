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
        return sentAt.ToLocalTime().ToString(TimeFormat, CultureInfo.CurrentCulture);
    }

    public static string FormatMoment(DateTime moment)
    {
        return moment.ToLocalTime().ToString(MomentFormat, CultureInfo.CurrentCulture);
    }

    public static DateTime ToLocalDay(DateTime moment)
    {
        return moment.ToLocalTime().Date;
    }

    public static string DescribeDay(DateTime sentAt, DateTime today)
    {
        DateTime day = ToLocalDay(sentAt);
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
        return ToLocalDay(sentAt) == today ? FormatTime(sentAt) : DescribeDay(sentAt, today);
    }
}
