using System.Text;
using Bastion.Server.Application.Security;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakePasswordHasher : IPasswordHasher
{
    public int VerifyCount { get; private set; }

    public static byte[] HashOf(string password)
    {
        return Encoding.UTF8.GetBytes(password);
    }

    public bool Verify(string password, byte[]? storedHash, byte[]? storedSalt)
    {
        VerifyCount++;
        return storedHash is not null && Encoding.UTF8.GetString(storedHash) == password;
    }
}
