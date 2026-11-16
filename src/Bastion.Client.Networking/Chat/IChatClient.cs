using System;
using Bastion.Client.Networking.Notifications;

namespace Bastion.Client.Networking.Chat;

public interface IChatClient
{
    event EventHandler<MatchChatMessageReceivedEventArgs>? MatchChatMessageReceived;

    DateTime SendLobbyChatMessage(string sessionToken, string text);

    DateTime SendMatchChatMessage(string sessionToken, int matchId, string text);

    LobbyChatTranscript OpenLobbyChat(string sessionToken);
}
