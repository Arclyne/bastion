using System;

namespace Bastion.Server.Application.Chat;

public interface IChatModerator
{
    string ScreenMessage(int accountId, string text, DateTime now);

    ChatMuteStatus FindMuteStatus(int accountId, DateTime now);
}
