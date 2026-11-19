using System.Collections.Generic;
using Bastion.Server.Application.Moderation;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeBannedWordRepository : IBannedWordRepository
{
    public FakeBannedWordRepository()
    {
        ChatTerms = [];
    }

    public List<string> ChatTerms { get; }

    public IReadOnlyList<string> ListChatTerms()
    {
        return ChatTerms;
    }
}
