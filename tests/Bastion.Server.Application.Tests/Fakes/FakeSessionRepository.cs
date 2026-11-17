using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Sessions;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeSessionRepository : ISessionRepository
{
    public FakeSessionRepository()
    {
        Sessions = new List<Session>();
    }

    public List<Session> Sessions { get; }

    public void Add(Session session)
    {
        Sessions.Add(session);
    }

    public Session? FindByTokenHash(byte[] tokenHash)
    {
        return Sessions.Find(session => session.TokenHash.SequenceEqual(tokenHash));
    }
}
