using System;

namespace Bastion.Client.Networking.Accounts;

public sealed class SignedInSession
{
    public SignedInSession(string token, string nickname, string preferredLanguage)
    {
        ArgumentNullException.ThrowIfNull(token);
        ArgumentNullException.ThrowIfNull(nickname);
        ArgumentNullException.ThrowIfNull(preferredLanguage);

        Token = token;
        Nickname = nickname;
        PreferredLanguage = preferredLanguage;
    }

    public string Token { get; }

    public string Nickname { get; }

    public string PreferredLanguage { get; }
}
