using Bastion.Domain.Auditing;

namespace Bastion.Server.Application.Auditing;

public interface IAccessLogRepository
{
    void Add(AccessLog accessLog);
}
