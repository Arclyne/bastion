using System.Collections.Generic;
using System.Globalization;
using Bastion.Server.Application.Rooms;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeRoomCodeGenerator : IRoomCodeGenerator
{
    private const string FallbackCodePrefix = "ZZZZ";
    private const int FallbackCodeDigits = 2;

    private int _fallbackCodeCount = 22;

    public Queue<string> Codes { get; } = new Queue<string>();

    public string CreateCode()
    {
        if (Codes.Count > 0)
        {
            return Codes.Dequeue();
        }

        _fallbackCodeCount++;
        return FallbackCodePrefix + _fallbackCodeCount.ToString("D" + FallbackCodeDigits, CultureInfo.InvariantCulture);
    }
}
