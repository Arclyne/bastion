using System.Collections.Generic;
using Bastion.Server.Application.Moderation;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeBannedWordRepository : IBannedWordRepository
{
    public List<string> ChatTerms { get; } = new List<string>();

    public IReadOnlyList<string> ListChatTerms()
    {
        return ChatTerms;
    }
}
