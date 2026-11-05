namespace Bastion.Client.Networking.Matchmaking;

public interface IMatchmakingClient
{
    void StartSearch(string sessionToken);

    void CancelSearch(string sessionToken);

    void ConfirmPresence(string sessionToken);
}
