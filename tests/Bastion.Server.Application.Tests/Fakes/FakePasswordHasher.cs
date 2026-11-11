using System.Text;
using Bastion.Server.Application.Security;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakePasswordHasher : IPasswordHasher
{
    public int SimulatedCheckCount { get; private set; }

    public static byte[] HashOf(string password)
    {
        return Encoding.UTF8.GetBytes(password);
    }

    public bool IsPasswordCorrect(string enteredPassword, byte[]? storedHash, byte[]? storedSalt)
    {
        return storedHash is not null && Encoding.UTF8.GetString(storedHash) == enteredPassword;
    }

    public void SimulatePasswordCheck(string enteredPassword)
    {
        SimulatedCheckCount++;
    }
}
