namespace Bastion.Server.Application.Security;

public interface IPasswordHasher
{
    bool Verify(string password, byte[]? storedHash, byte[]? storedSalt);
}
