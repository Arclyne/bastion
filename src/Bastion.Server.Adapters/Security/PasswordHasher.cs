using System.Security.Cryptography;
using System.Text;
using Bastion.Server.Application.Security;

namespace Bastion.Server.Adapters.Security;

public sealed class PasswordHasher : IPasswordHasher
{
    public const int HashLength = 64;
    public const int SaltLength = 32;

    private const int Iterations = 210_000;

    private static readonly byte[] _decoySalt = RandomNumberGenerator.GetBytes(SaltLength);
    private static readonly byte[] _decoyHash = RandomNumberGenerator.GetBytes(HashLength);

    public bool Verify(string password, byte[]? storedHash, byte[]? storedSalt)
    {
        bool hasStoredPassword = storedHash is not null && storedSalt is not null;
        byte[] salt = storedSalt ?? _decoySalt;
        byte[] expectedHash = storedHash ?? _decoyHash;

        byte[] derivedHash = Derive(password ?? string.Empty, salt);
        bool isMatch = CryptographicOperations.FixedTimeEquals(derivedHash, expectedHash);
        return hasStoredPassword && isMatch && !string.IsNullOrEmpty(password);
    }

    private static byte[] Derive(string password, byte[] salt)
    {
        return Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            Iterations,
            HashAlgorithmName.SHA512,
            HashLength);
    }
}
