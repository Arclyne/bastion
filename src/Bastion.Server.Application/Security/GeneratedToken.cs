using System;

namespace Bastion.Server.Application.Security;

public sealed class GeneratedToken
{
    public GeneratedToken(string token, byte[] tokenHash)
    {
        ArgumentNullException.ThrowIfNull(token);
        ArgumentNullException.ThrowIfNull(tokenHash);

        Token = token;
        TokenHash = tokenHash;
    }

    public string Token { get; }

    public byte[] TokenHash { get; }
}
