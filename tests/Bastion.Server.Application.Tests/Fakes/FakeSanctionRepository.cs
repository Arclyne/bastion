using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Moderation;
using Bastion.Server.Application.Moderation;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeSanctionRepository : ISanctionRepository
{
    private readonly List<Sanction> _sanctions;

    public FakeSanctionRepository()
    {
        _sanctions = new List<Sanction>();
    }

    public void AddSanction(Sanction sanction)
    {
        _sanctions.Add(sanction);
    }

    public IReadOnlyList<Sanction> ListChatSanctions(int accountId)
    {
        IEnumerable<Sanction> chatSanctions = _sanctions
            .Where(sanction => sanction.AccountId == accountId && sanction.Scope == SanctionScope.Chat);
        return new List<Sanction>(chatSanctions);
    }
}
