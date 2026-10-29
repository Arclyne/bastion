using Bastion.Server.Application.Security;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeSessionTokenGenerator : ISessionTokenGenerator
{
    public const string Token = "session-token";

    public GeneratedToken Generate()
    {
        return new GeneratedToken(Token, Hash(Token));
    }

    public byte[] Hash(string token)
    {
        return token == Token ? new byte[] { 1, 2, 3 } : new byte[] { 9 };
    }
}
