using Bastion.Domain.Accounts;
using Xunit;

namespace Bastion.Domain.Tests.Accounts;

public class TestAccountPolicy
{
    [Theory]
    [InlineData("marta_muros")]
    [InlineData("Ana")]
    [InlineData("jugador_con_30_caracteres_ok_1")]
    public void IsValidNickname_ValidNickname_ReturnsTrue(string nickname)
    {
        bool isValidNickname = AccountPolicy.IsValidNickname(nickname);

        Assert.True(isValidNickname);
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("marta muros")]
    [InlineData("marta@muros")]
    [InlineData("jugador_con_31_caracteres_ok_12")]
    [InlineData(null)]
    public void IsValidNickname_InvalidNickname_ReturnsFalse(string? nickname)
    {
        bool isValidNickname = AccountPolicy.IsValidNickname(nickname);

        Assert.False(isValidNickname);
    }

    [Theory]
    [InlineData("marta@example.com")]
    [InlineData("a.villalobos@correo-universitario.uv.mx")]
    public void IsValidEmail_ValidEmail_ReturnsTrue(string email)
    {
        bool isValidEmail = AccountPolicy.IsValidEmail(email);

        Assert.True(isValidEmail);
    }

    [Theory]
    [InlineData("marta@")]
    [InlineData("marta@example")]
    [InlineData("marta example@uv.mx")]
    public void IsValidEmail_InvalidEmail_ReturnsFalse(string email)
    {
        bool isValidEmail = AccountPolicy.IsValidEmail(email);

        Assert.False(isValidEmail);
    }

    [Theory]
    [InlineData("marta_muros")]
    [InlineData("marta@example.com")]
    public void IsValidLoginIdentifier_NicknameOrEmail_ReturnsTrue(string identifier)
    {
        bool isValidLoginIdentifier = AccountPolicy.IsValidLoginIdentifier(identifier);

        Assert.True(isValidLoginIdentifier);
    }

    [Fact]
    public void IsValidLoginIdentifier_NeitherNicknameNorEmail_ReturnsFalse()
    {
        bool isValidLoginIdentifier = AccountPolicy.IsValidLoginIdentifier("marta@");

        Assert.False(isValidLoginIdentifier);
    }
}
