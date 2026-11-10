using System;
using Bastion.Domain.Sessions;

namespace Bastion.Domain.Auditing;

public sealed record AccessAttempt
{
    public AccessAttempt(string enteredIdentifier, ClientConnection connection, DateTime occurredAt)
    {
        ArgumentNullException.ThrowIfNull(enteredIdentifier);
        ArgumentNullException.ThrowIfNull(connection);

        EnteredIdentifier = enteredIdentifier;
        Connection = connection;
        OccurredAt = occurredAt;
    }

    public string EnteredIdentifier { get; }

    public ClientConnection Connection { get; }

    public DateTime OccurredAt { get; }
}
