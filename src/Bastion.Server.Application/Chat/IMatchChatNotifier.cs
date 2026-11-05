using System.Collections.Generic;

namespace Bastion.Server.Application.Chat;

public interface IMatchChatNotifier
{
    void NotifyMatchChatMessage(int matchId, IReadOnlyCollection<int> recipientAccountIds, ChatLine line);
}
