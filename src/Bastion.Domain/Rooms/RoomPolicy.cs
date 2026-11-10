using System;
using System.Globalization;
using System.Linq;

namespace Bastion.Domain.Rooms;

public static class RoomPolicy
{
    public const int CodeLength = 6;
    public const string CodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    public const byte MinimumWallsPerPlayer = 1;
    public const byte MaximumWallsPerPlayer = 20;
    public const int MaximumFailedCodeAttempts = 5;

    private const int FailedCodeWindowMinutes = 5;

    private static readonly byte[] _clockMinuteOptions = new byte[] { 3, 5, 10 };

    public static TimeSpan FailedCodeWindow { get; } = TimeSpan.FromMinutes(FailedCodeWindowMinutes);

    public static string NormalizeCode(string code)
    {
        ArgumentNullException.ThrowIfNull(code);

        string trimmedCode = code.Trim();
        return trimmedCode.ToUpper(CultureInfo.InvariantCulture);
    }

    public static bool IsValidCode(string code)
    {
        ArgumentNullException.ThrowIfNull(code);

        return code.Length == CodeLength && code.All(CodeAlphabet.Contains);
    }

    public static bool IsValidWallCount(byte wallsPerPlayer)
    {
        return wallsPerPlayer >= MinimumWallsPerPlayer && wallsPerPlayer <= MaximumWallsPerPlayer;
    }

    public static bool IsValidClock(byte clockMinutes)
    {
        return _clockMinuteOptions.Contains(clockMinutes);
    }
}
