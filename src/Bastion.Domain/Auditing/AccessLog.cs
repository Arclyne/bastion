using System;

namespace Bastion.Domain.Auditing;

public class AccessLog
{
    public AccessLog(AccessLogResult result, AccessAttempt attempt, int? accountId)
    {
        ArgumentNullException.ThrowIfNull(attempt);

        Result = result;
        AccountId = accountId;
        EnteredIdentifier = attempt.EnteredIdentifier;
        IpAddress = attempt.Connection.IpAddress;
        OccurredAt = attempt.OccurredAt;
    }

    private AccessLog()
    {
        IpAddress = string.Empty;
    }

    public long AccessLogId { get; private set; }

    public int? AccountId { get; private set; }

    public string? EnteredIdentifier { get; private set; }

    public AccessLogResult Result { get; private set; }

    public string IpAddress { get; private set; }

    public DateTime OccurredAt { get; private set; }
}
