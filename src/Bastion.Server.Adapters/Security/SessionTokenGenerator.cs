using System;
using System.Security.Cryptography;
using System.Text;
using Bastion.Server.Application.Security;

namespace Bastion.Server.Adapters.Security;

public sealed class SessionTokenGenerator : ISessionTokenGenerator
{
    public const int TokenHashLength = SHA256.HashSizeInBytes;

    private const int TokenByteCount = 32;

    public GeneratedToken Generate()
    {
        byte[] randomValue = RandomNumberGenerator.GetBytes(TokenByteCount);
        string token = Convert.ToBase64String(randomValue);
        byte[] tokenHash = Hash(token);
        return new GeneratedToken(token, tokenHash);
    }

    public byte[] Hash(string token)
    {
        ArgumentNullException.ThrowIfNull(token);

        byte[] encodedToken = Encoding.UTF8.GetBytes(token);
        return SHA256.HashData(encodedToken);
    }
}
