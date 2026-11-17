using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Common;
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

    public Lookup<Session> FindByTokenHash(byte[] tokenHash)
    {
        Session? session = Sessions.Find(candidate => candidate.TokenHash.SequenceEqual(tokenHash));
        return Lookup.FromNullable(session, "Search=TokenHash");
    }
}
