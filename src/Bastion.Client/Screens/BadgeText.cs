using System.Globalization;

namespace Bastion.Client.Screens;

public static class BadgeText
{
    public static string FormatCount(int count)
    {
        if (count <= 0)
        {
            return string.Empty;
        }

        return count.ToString(CultureInfo.CurrentCulture);
    }
}
