using System.Collections.Generic;
using Bastion.Domain.Auditing;
using Bastion.Server.Application.Auditing;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeAccessLogRepository : IAccessLogRepository
{
    public FakeAccessLogRepository()
    {
        AccessLogs = [];
    }

    public List<AccessLog> AccessLogs { get; }

    public void Add(AccessLog accessLog)
    {
        AccessLogs.Add(accessLog);
    }
}
