using System;

namespace Bastion.Client.Screens.Matches;

public sealed class PlayerLeftEventArgs : EventArgs
{
    public PlayerLeftEventArgs(string nickname)
    {
        ArgumentNullException.ThrowIfNull(nickname);

        Nickname = nickname;
    }

    public string Nickname { get; }
}
