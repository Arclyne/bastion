using System;
using Bastion.Client.Networking.Notifications;

namespace Bastion.Client.Networking.Chat;

public interface IChatClient
{
    event EventHandler<MatchChatMessageReceivedEventArgs>? MatchChatMessageReceived;

    void SendLobbyChatMessage(string sessionToken, string text);

    void SendMatchChatMessage(string sessionToken, int matchId, string text);
}
