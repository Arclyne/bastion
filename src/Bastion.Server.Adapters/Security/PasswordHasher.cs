using System;
using System.Security.Cryptography;
using System.Text;
using Bastion.Server.Application.Security;

namespace Bastion.Server.Adapters.Security;

public sealed class PasswordHasher : IPasswordHasher
{
    public const int HashLength = 64;
    public const int SaltLength = 32;

    private const int Iterations = 210_000;

    private static readonly byte[] _saltWhenNoPasswordStored = RandomNumberGenerator.GetBytes(SaltLength);
    private static readonly byte[] _hashWhenNoPasswordStored = RandomNumberGenerator.GetBytes(HashLength);

    public bool IsPasswordCorrect(string enteredPassword, byte[]? storedHash, byte[]? storedSalt)
    {
        ArgumentNullException.ThrowIfNull(enteredPassword);

        bool isHashEqual = IsHashEqualWithoutTimingLeak(enteredPassword, storedHash, storedSalt);
        bool hasStoredPassword = storedHash is not null && storedSalt is not null;
        bool isPasswordEntered = enteredPassword.Length > 0;
        return hasStoredPassword && isPasswordEntered && isHashEqual;
    }

    public void SimulatePasswordCheck(string enteredPassword)
    {
        ArgumentNullException.ThrowIfNull(enteredPassword);

        IsHashEqualWithoutTimingLeak(enteredPassword, null, null);
    }

    private static bool IsHashEqualWithoutTimingLeak(string enteredPassword, byte[]? storedHash, byte[]? storedSalt)
    {
        byte[] salt = storedSalt ?? _saltWhenNoPasswordStored;
        byte[] expectedHash = storedHash ?? _hashWhenNoPasswordStored;
        byte[] enteredHash = DeriveHash(enteredPassword, salt);
        return CryptographicOperations.FixedTimeEquals(enteredHash, expectedHash);
    }

    private static byte[] DeriveHash(string password, byte[] salt)
    {
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        return Rfc2898DeriveBytes.Pbkdf2(
            passwordBytes,
            salt,
            Iterations,
            HashAlgorithmName.SHA512,
            HashLength);
    }
}
