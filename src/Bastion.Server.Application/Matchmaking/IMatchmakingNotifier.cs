namespace Bastion.Server.Application.Matchmaking;

public interface IMatchmakingNotifier
{
    bool HasNotificationChannel(int accountId);

    void NotifyMatchFound(CreatedMatch createdMatch);

    void NotifyPresenceRequested(PresenceRequest request);

    void NotifySearchEnded(int accountId, SearchEndReason reason);
}
