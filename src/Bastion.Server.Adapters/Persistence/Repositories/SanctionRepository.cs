using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Moderation;
using Bastion.Server.Application.Moderation;
using Microsoft.EntityFrameworkCore;

namespace Bastion.Server.Adapters.Persistence.Repositories;

public sealed class SanctionRepository : ISanctionRepository
{
    private readonly BastionDbContext _context;

    public SanctionRepository(BastionDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;
    }

    public IReadOnlyList<Sanction> ListChatSanctions(int accountId)
    {
        return _context.Sanctions
            .AsNoTracking()
            .Where(sanction => sanction.AccountId == accountId && sanction.Scope == SanctionScope.Chat)
            .ToList();
    }
}
