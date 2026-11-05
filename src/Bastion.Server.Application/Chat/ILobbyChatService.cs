using System;

namespace Bastion.Server.Application.Chat;

public interface ILobbyChatService
{
    void SendMessage(string sessionToken, string text, DateTime now);
}
