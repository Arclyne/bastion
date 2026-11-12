using System;
using Bastion.Domain.Matches;

namespace Bastion.Client.Screens.Matches;

public sealed record VersusPlayer
{
    public VersusPlayer(string nickname, string eloText, PawnSymbol pawnSymbol)
    {
        ArgumentNullException.ThrowIfNull(nickname);
        ArgumentNullException.ThrowIfNull(eloText);

        Nickname = nickname;
        EloText = eloText;
        PawnSymbol = pawnSymbol;
    }

    public string Nickname { get; }

    public string EloText { get; }

    public PawnSymbol PawnSymbol { get; }
}
