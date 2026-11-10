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
    public void HasBannedTerm_TextWithTerm_ReturnsTrue(string text)
    {
        bool hasBannedTerm = ChatWordFilter.HasBannedTerm(text, _bannedTerms);

        Assert.True(hasBannedTerm);
    }

    [Theory]
    [InlineData("buena partida")]
    [InlineData("qué tontería")]
    [InlineData("tontos")]
    [InlineData("mala jugada, palabra")]
    public void HasBannedTerm_TextWithoutWholeTerm_ReturnsFalse(string text)
    {
        bool hasBannedTerm = ChatWordFilter.HasBannedTerm(text, _bannedTerms);

        Assert.False(hasBannedTerm);
    }

    [Fact]
    public void HasBannedTerm_BlankTerm_ReturnsFalse()
    {
        bool hasBannedTerm = ChatWordFilter.HasBannedTerm("hola", new[] { "  " });

        Assert.False(hasBannedTerm);
    }
}
