namespace Bastion.Server.Application.Security;

public interface ISessionTokenGenerator
{
    GeneratedToken Generate();

    byte[] Hash(string token);
}
