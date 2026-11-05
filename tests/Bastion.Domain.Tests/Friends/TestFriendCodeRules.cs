using Bastion.Domain.Friends;
using Xunit;

namespace Bastion.Domain.Tests.Friends;

public class TestFriendCodeRules
{
    [Theory]
    [InlineData("QU7K2P")]
    [InlineData("AB2CD9")]
    public void IsValidFriendCode_ValidCode_ReturnsTrue(string text)
    {
        bool isValidFriendCode = FriendCodeRules.IsValidFriendCode(text);

        Assert.True(isValidFriendCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("QU7K2")]
    [InlineData("QU7K2PX")]
    [InlineData("QU0K2P")]
    [InlineData("QUOK2P")]
    [InlineData("QU1K2P")]
    [InlineData("QUIK2P")]
    [InlineData("QULK2P")]
    [InlineData("qu7k2p")]
    [InlineData("QU-7K2P")]
    public void IsValidFriendCode_InvalidCode_ReturnsFalse(string? text)
    {
        bool isValidFriendCode = FriendCodeRules.IsValidFriendCode(text);

        Assert.False(isValidFriendCode);
    }

    [Fact]
    public void NormalizeFriendCode_HyphenatedLowercase_ReturnsCompactUppercase()
    {
        string normalizedCode = FriendCodeRules.NormalizeFriendCode("qu-7k2p");

        Assert.Equal("QU7K2P", normalizedCode);
    }

    [Fact]
    public void NormalizeFriendCode_SurroundingSpaces_ReturnsTrimmedCode()
    {
        string normalizedCode = FriendCodeRules.NormalizeFriendCode("  QU7K2P ");

        Assert.Equal("QU7K2P", normalizedCode);
    }

    [Fact]
    public void FormatFriendCode_ValidCode_InsertsHyphenAfterTwo()
    {
        string formattedCode = FriendCodeRules.FormatFriendCode("QU7K2P");

        Assert.Equal("QU-7K2P", formattedCode);
    }
}
