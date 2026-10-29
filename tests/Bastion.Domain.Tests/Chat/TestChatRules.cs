using Bastion.Domain.Chat;
using Xunit;

namespace Bastion.Domain.Tests.Chat;

public class TestChatRules
{
    [Theory]
    [InlineData("hola")]
    [InlineData("  hola  ")]
    public void IsMessage_ValidMessage_ReturnsTrue(string text)
    {
        bool isMessage = ChatRules.IsMessage(text);

        Assert.True(isMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsMessage_InvalidMessage_ReturnsFalse(string? text)
    {
        bool isMessage = ChatRules.IsMessage(text);

        Assert.False(isMessage);
    }

    [Fact]
    public void IsMessage_MaximumLength_ReturnsTrue()
    {
        string text = new string('a', ChatRules.MaximumMessageLength);

        bool isMessage = ChatRules.IsMessage(text);

        Assert.True(isMessage);
    }

    [Fact]
    public void IsMessage_AboveMaximumLength_ReturnsFalse()
    {
        string text = new string('a', ChatRules.MaximumMessageLength + 1);

        bool isMessage = ChatRules.IsMessage(text);

        Assert.False(isMessage);
    }
}
