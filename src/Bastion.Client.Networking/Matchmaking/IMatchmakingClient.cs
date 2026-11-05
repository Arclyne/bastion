using System;
using Bastion.Client.Networking.Notifications;

namespace Bastion.Client.Networking.Matchmaking;

public interface IMatchmakingClient
{
    event EventHandler<MatchFoundEventArgs>? MatchFound;

    event EventHandler<PresenceRequestedEventArgs>? PresenceRequested;

    event EventHandler<SearchEndedEventArgs>? SearchEnded;

    event EventHandler? Disconnected;

    void StartSearch(string sessionToken);

    void CancelSearch(string sessionToken);

    void ConfirmPresence(string sessionToken);
}
