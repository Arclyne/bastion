using System;
using Bastion.Domain.Sessions;

namespace Bastion.Domain.Auditing;

public sealed record AccessAttempt(string EnteredIdentifier, ClientConnection Connection, DateTime OccurredAt);
