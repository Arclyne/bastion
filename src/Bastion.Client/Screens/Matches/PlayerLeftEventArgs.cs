using System;
using Bastion.Client.Networking.Matches;

namespace Bastion.Client.Screens.Matches;

public sealed class PlayerLeftEventArgs : EventArgs
{
    public PlayerLeftEventArgs(string nickname, EndedMatchReason endReason)
    {
        ArgumentNullException.ThrowIfNull(nickname);

        Nickname = nickname;
        EndReason = endReason;
    }

    public string Nickname { get; }

    public EndedMatchReason EndReason { get; }
}
