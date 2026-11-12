using Bastion.Server.Application.Security;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeSessionTokenGenerator : ISessionTokenGenerator
{
    public const string Token = "session-token";

    private static readonly byte[] _tokenHash = { 1, 2, 3 };
    private static readonly byte[] _otherTokenHash = { 9 };

    public GeneratedToken Generate()
    {
        byte[] tokenHash = Hash(Token);
        return new GeneratedToken(Token, tokenHash);
    }

    public byte[] Hash(string token)
    {
        if (token == Token)
        {
            return _tokenHash;
        }

        return _otherTokenHash;
    }
}
