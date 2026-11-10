using System;

namespace Bastion.Server.Application.Matches;

public sealed record AbandonmentNotice
{
    public AbandonmentNotice(string abandoningNickname, bool isMatchOver)
    {
        ArgumentNullException.ThrowIfNull(abandoningNickname);

        AbandoningNickname = abandoningNickname;
        IsMatchOver = isMatchOver;
    }

    public string AbandoningNickname { get; }

    public bool IsMatchOver { get; }
}
