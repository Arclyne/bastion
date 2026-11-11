namespace Bastion.Server.Application.Security;

public interface IPasswordHasher
{
    bool IsPasswordCorrect(string enteredPassword, byte[]? storedHash, byte[]? storedSalt);

    void SimulatePasswordCheck(string enteredPassword);
}
