using Bastion.Domain.Accounts;
using Xunit;

namespace Bastion.Domain.Tests.Accounts;

public class TestAccountRules
{
    [Theory]
    [InlineData("marta_muros")]
    [InlineData("Ana")]
    [InlineData("jugador_con_30_caracteres_ok_1")]
    public void IsNickname_ValidNickname_ReturnsTrue(string nickname)
    {
        bool isNickname = AccountRules.IsNickname(nickname);

        Assert.True(isNickname);
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("marta muros")]
    [InlineData("marta@muros")]
    [InlineData("jugador_con_31_caracteres_ok_12")]
    [InlineData(null)]
    public void IsNickname_InvalidNickname_ReturnsFalse(string? nickname)
    {
        bool isNickname = AccountRules.IsNickname(nickname);

        Assert.False(isNickname);
    }

    [Theory]
    [InlineData("marta@example.com")]
    [InlineData("a.villalobos@correo-universitario.uv.mx")]
    public void IsEmail_ValidEmail_ReturnsTrue(string email)
    {
        bool isEmail = AccountRules.IsEmail(email);

        Assert.True(isEmail);
    }

    [Theory]
    [InlineData("marta@")]
    [InlineData("marta@example")]
    [InlineData("marta example@uv.mx")]
    public void IsEmail_InvalidEmail_ReturnsFalse(string email)
    {
        bool isEmail = AccountRules.IsEmail(email);

        Assert.False(isEmail);
    }

    [Theory]
    [InlineData("marta_muros")]
    [InlineData("marta@example.com")]
    public void IsIdentifier_NicknameOrEmail_ReturnsTrue(string identifier)
    {
        bool isIdentifier = AccountRules.IsIdentifier(identifier);

        Assert.True(isIdentifier);
    }

    [Fact]
    public void IsIdentifier_NeitherNicknameNorEmail_ReturnsFalse()
    {
        bool isIdentifier = AccountRules.IsIdentifier("marta@");

        Assert.False(isIdentifier);
    }
}
