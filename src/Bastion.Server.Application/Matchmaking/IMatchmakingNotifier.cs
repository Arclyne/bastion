namespace Bastion.Server.Application.Matchmaking;

public interface IMatchmakingNotifier
{
    bool CanNotify(int accountId);

    void NotifyMatchFound(MatchPairing pairing);

    void RequestPresence(PresenceRequest request);

    void NotifySearchEnded(int accountId, SearchEndReason reason);
}
