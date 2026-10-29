using System;

namespace Bastion.Server.Application.Chat;

public interface ILobbyChatService
{
    void Send(string sessionToken, string text, DateTime now);
}
