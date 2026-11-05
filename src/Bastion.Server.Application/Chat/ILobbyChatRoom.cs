using System;
using System.Collections.Generic;

namespace Bastion.Server.Application.Chat;

public interface ILobbyChatRoom
{
    bool TryClaimTurn(int accountId, DateTime now);

    PostedChatLine Post(ChatLine line);

    IReadOnlyList<PostedChatLine> ListRecent(int count);

    int CountListeners();
}
