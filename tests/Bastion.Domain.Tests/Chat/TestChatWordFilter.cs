using Bastion.Domain.Chat;
using Xunit;

namespace Bastion.Domain.Tests.Chat;

public class TestChatWordFilter
{
    private static readonly string[] _bannedTerms = { "tonto", "estúpido", "mala palabra" };

    [Theory]
    [InlineData("eres un tonto")]
    [InlineData("TONTO!!")]
    [InlineData("que estupido eres")]
    [InlineData("Eres ESTÚPIDO.")]
    [InlineData("dijo una mala   palabra")]
    public void ContainsBannedTerm_TextWithTerm_ReturnsTrue(string text)
    {
        bool containsBannedTerm = ChatWordFilter.ContainsBannedTerm(text, _bannedTerms);

        Assert.True(containsBannedTerm);
    }

    [Theory]
    [InlineData("buena partida")]
    [InlineData("qué tontería")]
    [InlineData("tontos")]
    [InlineData("mala jugada, palabra")]
    public void ContainsBannedTerm_TextWithoutWholeTerm_ReturnsFalse(string text)
    {
        bool containsBannedTerm = ChatWordFilter.ContainsBannedTerm(text, _bannedTerms);

        Assert.False(containsBannedTerm);
    }

    [Fact]
    public void ContainsBannedTerm_BlankTerm_ReturnsFalse()
    {
        bool containsBannedTerm = ChatWordFilter.ContainsBannedTerm("hola", new[] { "  " });

        Assert.False(containsBannedTerm);
    }
}
