using Bastion.Domain.Chat;
using Xunit;

namespace Bastion.Domain.Tests.Chat;

public class TestChatRules
{
    [Theory]
    [InlineData("hola")]
    [InlineData("  hola  ")]
    public void IsValidMessage_ValidMessage_ReturnsTrue(string text)
    {
        bool isValidMessage = ChatRules.IsValidMessage(text);

        Assert.True(isValidMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValidMessage_InvalidMessage_ReturnsFalse(string? text)
    {
        bool isValidMessage = ChatRules.IsValidMessage(text);

        Assert.False(isValidMessage);
    }

    [Fact]
    public void IsValidMessage_MaximumLength_ReturnsTrue()
    {
        string text = new string('a', ChatRules.MaximumMessageLength);

        bool isValidMessage = ChatRules.IsValidMessage(text);

        Assert.True(isValidMessage);
    }

    [Fact]
    public void IsValidMessage_AboveMaximumLength_ReturnsFalse()
    {
        string text = new string('a', ChatRules.MaximumMessageLength + 1);

        bool isValidMessage = ChatRules.IsValidMessage(text);

        Assert.False(isValidMessage);
    }
}
