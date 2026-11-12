using System;

namespace Bastion.Client.Networking.Chat;

public sealed record ChatAuthor
{
    public ChatAuthor(string nickname, bool isOwn)
    {
        ArgumentNullException.ThrowIfNull(nickname);

        Nickname = nickname;
        IsOwn = isOwn;
    }

    public string Nickname { get; }

    public bool IsOwn { get; }
}
