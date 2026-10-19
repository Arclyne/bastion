using System;

namespace Bastion.Client.Networking.Accounts;

public sealed class LoginCredentials
{
    public LoginCredentials(string identifier, string password)
    {
        ArgumentNullException.ThrowIfNull(identifier);
        ArgumentNullException.ThrowIfNull(password);

        Identifier = identifier;
        Password = password;
    }

    public string Identifier { get; }

    public string Password { get; }
}
