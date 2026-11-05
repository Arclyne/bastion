using System;
using System.Collections.Generic;

namespace Bastion.Server.Application.Chat;

public interface IMatchChatRelay
{
    IReadOnlyList<int> ListParticipantAccountIds(int matchId);

    bool TryStartCooldown(int accountId, DateTime now);

    PostedChatLine Post(int matchId, ChatLine line, IReadOnlyCollection<int> recipientAccountIds);
}
