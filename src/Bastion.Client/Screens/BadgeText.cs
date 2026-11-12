using System.Globalization;

namespace Bastion.Client.Screens;

public static class BadgeText
{
    private const int NoItems = 0;

    public static string FormatCount(int count)
    {
        if (count <= NoItems)
        {
            return string.Empty;
        }

        return count.ToString(CultureInfo.CurrentCulture);
    }
}
