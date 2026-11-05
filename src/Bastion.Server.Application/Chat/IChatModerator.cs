using System;

namespace Bastion.Server.Application.Chat;

public interface IChatModerator
{
    string Screen(int accountId, string text, DateTime now);

    ChatMute FindMute(int accountId, DateTime now);
}
