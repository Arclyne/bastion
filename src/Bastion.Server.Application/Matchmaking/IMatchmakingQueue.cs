using System;
using Bastion.Domain.Common;
using Bastion.Domain.Matchmaking;

namespace Bastion.Server.Application.Matchmaking;

public interface IMatchmakingQueue
{
    bool TryAdd(MatchSearch matchSearch);

    bool TryRemove(int accountId);

    bool IsSearching(int accountId);

    Lookup<DateTime> FindSearchStart(int accountId);

    bool IsAwaitingPresence(int accountId);

    void ConfirmPresence(int accountId, DateTime now);

    QueueReview Review(DateTime now);
}
