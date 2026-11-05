using System.Collections.Generic;
using Bastion.Domain.Moderation;

namespace Bastion.Server.Application.Moderation;

public interface ISanctionRepository
{
    IReadOnlyList<Sanction> ListChatSanctions(int accountId);
}
