using System;
using System.Security.Cryptography;
using System.Text;
using Bastion.Server.Application.Security;

namespace Bastion.Server.Adapters.Security;

public sealed class SessionTokenGenerator : ISessionTokenGenerator
{
    private const int TokenLength = 32;

    public GeneratedToken Generate()
    {
        string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(TokenLength));
        return new GeneratedToken(token, Hash(token));
    }

    public byte[] Hash(string token)
    {
        ArgumentNullException.ThrowIfNull(token);

        return SHA256.HashData(Encoding.UTF8.GetBytes(token));
    }
}
