using System;

namespace Bastion.Server.Application.Chat;

public interface ILobbyChatService
{
    PostedChatLine SendMessage(string sessionToken, string text, DateTime now);

    LobbyChatHistory OpenLobbyChat(string sessionToken, DateTime now);
}
