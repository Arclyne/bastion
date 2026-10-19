using System;

namespace Bastion.Domain.Sessions;

public sealed record SessionTicket(byte[] TokenHash, DateTime IssuedAt);
