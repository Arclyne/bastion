using Bastion.Domain.Matches;

namespace Bastion.Client.Screens.Matches;

public sealed record VersusPlayer(string Nickname, string EloText, PawnSymbol PawnSymbol);
