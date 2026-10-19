using Bastion.Domain.Sessions;

namespace Bastion.Server.Application.Sessions;

public interface ISessionRepository
{
    void Add(Session session);
}
