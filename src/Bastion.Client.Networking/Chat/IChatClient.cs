using System;

namespace Bastion.Client.Networking.Chat;

public interface IChatClient
{
    DateTime SendMessage(string sessionToken, string text);

    LobbyChatTranscript OpenLobbyChat(string sessionToken);
}
