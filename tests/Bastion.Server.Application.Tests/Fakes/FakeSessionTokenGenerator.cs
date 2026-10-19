using Bastion.Server.Application.Security;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeSessionTokenGenerator : ISessionTokenGenerator
{
    public const string Token = "session-token";

    public GeneratedToken Generate()
    {
        return new GeneratedToken(Token, new byte[] { 1, 2, 3 });
    }
}
