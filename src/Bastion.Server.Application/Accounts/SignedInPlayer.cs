using System;

namespace Bastion.Server.Application.Accounts;

public sealed class SignedInPlayer
{
    public SignedInPlayer(string sessionToken, string nickname, string preferredLanguage)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(nickname);
        ArgumentNullException.ThrowIfNull(preferredLanguage);

        SessionToken = sessionToken;
        Nickname = nickname;
        PreferredLanguage = preferredLanguage;
    }

    public string SessionToken { get; }

    public string Nickname { get; }

    public string PreferredLanguage { get; }
}
