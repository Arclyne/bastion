using System.Linq;
using System;
using Bastion.Domain.Common;
using Bastion.Domain.Sessions;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Adapters.Persistence.Repositories;

public sealed class SessionRepository : ISessionRepository
{
    private readonly BastionDbContext _context;

    public SessionRepository(BastionDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;
    }

    public void Add(Session session)
    {
        ArgumentNullException.ThrowIfNull(session);

        _context.Sessions.Add(session);
    }

    public Lookup<Session> FindByTokenHash(byte[] tokenHash)
    {
        ArgumentNullException.ThrowIfNull(tokenHash);

        Session? matchingSession = _context.Sessions.FirstOrDefault(session => session.TokenHash == tokenHash);
        return Lookup.FromNullable(matchingSession, "Search=TokenHash");
    }
}
