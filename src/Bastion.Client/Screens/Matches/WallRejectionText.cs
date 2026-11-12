using System;
using System.Globalization;
using Bastion.Client.Localization;
using Bastion.Domain.Matches;

namespace Bastion.Client.Screens.Matches;

public static class WallRejectionText
{
    public static string Describe(WallRejectionReason reason, string blockedNickname)
    {
        ArgumentNullException.ThrowIfNull(blockedNickname);

        switch (reason)
        {
            case WallRejectionReason.NoWallsLeft:
                return Strings.MatchWallNoWallsLeft;
            case WallRejectionReason.OutOfBoard:
                return Strings.MatchWallOutOfBoard;
            case WallRejectionReason.GrooveTaken:
                return Strings.MatchWallGrooveTaken;
            case WallRejectionReason.WouldBlockPlayer:
                return string.Format(CultureInfo.CurrentCulture, Strings.MatchWallWouldBlockFormat, blockedNickname);
            default:
                throw new ArgumentOutOfRangeException(nameof(reason), reason, "Unsupported wall rejection reason.");
        }
    }
}
