using Bastion.Domain.Rooms;
using Xunit;

namespace Bastion.Domain.Tests.Rooms;

public class TestRoomRules
{
    private const string ValidCode = "K7MTQ3";
    private const string CodeWithLetterO = "K7MTO3";
    private const string CodeWithDigitOne = "K7MT13";
    private const string ShortCode = "K7MTQ";
    private const string LowercaseCode = "k7mtq3";
    private const byte TooManyWalls = 21;
    private const byte NoWalls = 0;
    private const byte FourMinutes = 4;
    private const byte TenMinutes = 10;

    [Fact]
    public void IsValidCode_SixAllowedCharacters_ReturnsTrue()
    {
        bool isValid = RoomRules.IsValidCode(ValidCode);

        Assert.True(isValid);
    }

    [Fact]
    public void IsValidCode_LetterO_ReturnsFalse()
    {
        bool isValid = RoomRules.IsValidCode(CodeWithLetterO);

        Assert.False(isValid);
    }

    [Fact]
    public void IsValidCode_DigitOne_ReturnsFalse()
    {
        bool isValid = RoomRules.IsValidCode(CodeWithDigitOne);

        Assert.False(isValid);
    }

    [Fact]
    public void IsValidCode_FiveCharacters_ReturnsFalse()
    {
        bool isValid = RoomRules.IsValidCode(ShortCode);

        Assert.False(isValid);
    }

    [Fact]
    public void IsValidCode_Lowercase_ReturnsFalse()
    {
        bool isValid = RoomRules.IsValidCode(LowercaseCode);

        Assert.False(isValid);
    }

    [Fact]
    public void IsValidWallCount_TwentyOne_ReturnsFalse()
    {
        bool isValid = RoomRules.IsValidWallCount(TooManyWalls);

        Assert.False(isValid);
    }

    [Fact]
    public void IsValidWallCount_Zero_ReturnsFalse()
    {
        bool isValid = RoomRules.IsValidWallCount(NoWalls);

        Assert.False(isValid);
    }

    [Fact]
    public void IsValidWallCount_Twenty_ReturnsTrue()
    {
        bool isValid = RoomRules.IsValidWallCount(RoomRules.MaximumWallsPerPlayer);

        Assert.True(isValid);
    }

    [Fact]
    public void IsValidClock_FourMinutes_ReturnsFalse()
    {
        bool isValid = RoomRules.IsValidClock(FourMinutes);

        Assert.False(isValid);
    }

    [Fact]
    public void IsValidClock_TenMinutes_ReturnsTrue()
    {
        bool isValid = RoomRules.IsValidClock(TenMinutes);

        Assert.True(isValid);
    }
}
