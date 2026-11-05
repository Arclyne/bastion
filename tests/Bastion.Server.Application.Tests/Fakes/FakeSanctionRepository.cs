using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Moderation;
using Bastion.Server.Application.Moderation;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeSanctionRepository : ISanctionRepository
{
    private readonly List<Sanction> _sanctions = new List<Sanction>();

    public void Store(Sanction sanction)
    {
        ArgumentNullException.ThrowIfNull(sanction);

        _sanctions.Add(sanction);
    }

    public IReadOnlyList<Sanction> ListChatSanctions(int accountId)
    {
        return _sanctions
            .Where(sanction => sanction.AccountId == accountId && sanction.Scope == SanctionScope.Chat)
            .ToList();
    }
}
