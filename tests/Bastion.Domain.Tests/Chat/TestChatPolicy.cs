using Bastion.Domain.Chat;
using Xunit;

namespace Bastion.Domain.Tests.Chat;

public sealed class TestChatPolicy
{
    private const char FillerCharacter = 'a';

    [Theory]
    [InlineData("hola")]
    [InlineData("  hola  ")]
    public void IsValidMessage_ValidMessage_ReturnsTrue(string text)
    {
        bool isValidMessage = ChatPolicy.IsValidMessage(text);

        Assert.True(isValidMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValidMessage_InvalidMessage_ReturnsFalse(string? text)
    {
        bool isValidMessage = ChatPolicy.IsValidMessage(text);

        Assert.False(isValidMessage);
    }

    [Fact]
    public void IsValidMessage_MaximumLength_ReturnsTrue()
    {
        string text = new string(FillerCharacter, ChatPolicy.MaximumMessageLength);

        bool isValidMessage = ChatPolicy.IsValidMessage(text);

        Assert.True(isValidMessage);
    }

    [Fact]
    public void IsValidMessage_AboveMaximumLength_ReturnsFalse()
    {
        int tooLongLength = ChatPolicy.MaximumMessageLength + 1;
        string text = new string(FillerCharacter, tooLongLength);

        bool isValidMessage = ChatPolicy.IsValidMessage(text);

        Assert.False(isValidMessage);
    }
}
