using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Matchmaking;

public sealed record CreatedMatchParticipant(ConnectedPlayer Player, short? EloRating, byte TurnOrder);
