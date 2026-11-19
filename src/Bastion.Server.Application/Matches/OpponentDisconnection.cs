using System;

namespace Bastion.Server.Application.Matches;

public sealed record OpponentDisconnection(byte TurnOrder, DateTime DisconnectedAt);
