using System.Collections.Generic;
using Bastion.Domain.Auditing;
using Bastion.Server.Application.Auditing;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeAccessLogRepository : IAccessLogRepository
{
    public List<AccessLog> AccessLogs { get; } = new List<AccessLog>();

    public void Add(AccessLog accessLog)
    {
        AccessLogs.Add(accessLog);
    }
}
