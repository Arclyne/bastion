using System;

namespace Bastion.Client.Networking.Friends;

public sealed record PlayerTag
{
    public PlayerTag(string nickname, string friendCode)
    {
        ArgumentNullException.ThrowIfNull(nickname);
        ArgumentNullException.ThrowIfNull(friendCode);

        Nickname = nickname;
        FriendCode = friendCode;
    }

    public string Nickname { get; }

    public string FriendCode { get; }
}
