namespace Bastion.Server.Application.Matchmaking;

public interface IMatchmakingNotifier
{
    bool HasNotificationChannel(int accountId);

    void NotifyMatchFound(MatchPairing pairing);

    void NotifyPresenceRequested(PresenceRequest request);

    void NotifySearchEnded(int accountId, SearchEndReason reason);
}
