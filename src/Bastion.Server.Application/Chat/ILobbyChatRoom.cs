using System;
using System.Collections.Generic;

namespace Bastion.Server.Application.Chat;

public interface ILobbyChatRoom
{
    bool TryStartCooldown(int accountId, DateTime now);

    PostedChatLine Post(ChatLine line);

    IReadOnlyList<PostedChatLine> ListRecent();

    int CountListeners();
}
