using Bastion.Domain.Friends;
using Xunit;

namespace Bastion.Domain.Tests.Friends;

public sealed class TestFriendCodePolicy
{
    private const string CompactCode = "QU7K2P";
    private const string HyphenatedCode = "QU-7K2P";
    private const string HyphenatedLowercaseCode = "qu-7k2p";
    private const string PaddedCode = "  QU7K2P ";

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
        string normalizedCode = FriendCodePolicy.Normalize(HyphenatedLowercaseCode);

        Assert.Equal(CompactCode, normalizedCode);
    }

    [Fact]
    public void Normalize_SurroundingSpaces_ReturnsTrimmedCode()
    {
        string normalizedCode = FriendCodePolicy.Normalize(PaddedCode);

        Assert.Equal(CompactCode, normalizedCode);
    }

    [Fact]
    public void Format_ValidCode_InsertsHyphenAfterTwo()
    {
        string formattedCode = FriendCodePolicy.Format(CompactCode);

        Assert.Equal(HyphenatedCode, formattedCode);
    }
}
