using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Moderation;
using Bastion.Server.Application.Moderation;

namespace Bastion.Server.Adapters.Persistence.Repositories;

public sealed class BannedWordRepository : IBannedWordRepository
{
    private readonly BastionDbContext _context;

    public BannedWordRepository(BastionDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;
    }

    public IReadOnlyList<string> ListChatTerms()
    {
        IQueryable<string> chatTerms = _context.BannedWords
            .Where(bannedWord => bannedWord.Scope == BannedWordScope.Chat || bannedWord.Scope == BannedWordScope.Both)
            .Select(bannedWord => bannedWord.Term);
        return new List<string>(chatTerms);
    }
}
