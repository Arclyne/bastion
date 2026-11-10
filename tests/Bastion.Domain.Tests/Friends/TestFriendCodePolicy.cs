using Bastion.Domain.Friends;
using Xunit;

namespace Bastion.Domain.Tests.Friends;

public class TestFriendCodePolicy
{
    [Theory]
    [InlineData("QU7K2P")]
    [InlineData("AB2CD9")]
    public void IsValid_ValidCode_ReturnsTrue(string text)
    {
        bool isValidFriendCode = FriendCodePolicy.IsValid(text);

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
    public void IsValid_InvalidCode_ReturnsFalse(string? text)
    {
        bool isValidFriendCode = FriendCodePolicy.IsValid(text);

        Assert.False(isValidFriendCode);
    }

    [Fact]
    public void Normalize_HyphenatedLowercase_ReturnsCompactUppercase()
    {
        string normalizedCode = FriendCodePolicy.Normalize("qu-7k2p");

        Assert.Equal("QU7K2P", normalizedCode);
    }

    [Fact]
    public void Normalize_SurroundingSpaces_ReturnsTrimmedCode()
    {
        string normalizedCode = FriendCodePolicy.Normalize("  QU7K2P ");

        Assert.Equal("QU7K2P", normalizedCode);
    }

    [Fact]
    public void Format_ValidCode_InsertsHyphenAfterTwo()
    {
        string formattedCode = FriendCodePolicy.Format("QU7K2P");

        Assert.Equal("QU-7K2P", formattedCode);
    }
}
