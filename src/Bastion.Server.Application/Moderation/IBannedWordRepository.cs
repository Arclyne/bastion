using System.Collections.Generic;

namespace Bastion.Server.Application.Moderation;

public interface IBannedWordRepository
{
    IReadOnlyList<string> ListChatTerms();
}
