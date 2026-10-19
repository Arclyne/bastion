using System;
using Bastion.Domain.Auditing;
using Bastion.Server.Application.Auditing;

namespace Bastion.Server.Adapters.Persistence.Repositories;

public sealed class AccessLogRepository : IAccessLogRepository
{
    private readonly BastionDbContext _context;

    public AccessLogRepository(BastionDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;
    }

    public void Add(AccessLog accessLog)
    {
        ArgumentNullException.ThrowIfNull(accessLog);

        _context.AccessLogs.Add(accessLog);
    }
}
