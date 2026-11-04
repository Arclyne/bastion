using System;

namespace Bastion.Server.Application.Matchmaking;

public sealed record PresenceRequest(int AccountId, DateTime Deadline);
