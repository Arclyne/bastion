using System;
using System.Collections.Generic;
using Bastion.Domain.Chat;
using Bastion.Domain.Common;
using Bastion.Domain.GameModes;
using Bastion.Domain.Matches;
using Bastion.Domain.Statistics;

namespace Bastion.Server.Application.Matches;

public interface IMatchRepository
{
    bool HasUnfinishedParticipation(int accountId);

    Lookup<GameMode> FindActiveGameMode(GameModeCode code);

    IReadOnlyDictionary<int, short> FindEloRatingsByAccountId(byte gameModeId, IReadOnlyCollection<int> accountIds);

    int SaveMatch(Match match);

    IReadOnlyList<int> FindAccountIdsWithUnfinishedParticipation(int matchId);

    long SaveMatchChatMessage(Message message);

    Lookup<Match> FindMatch(int matchId);

    void SaveDeparture(Match match);

    Lookup<GameMode> FindGameMode(byte gameModeId);

    void SaveMove(Match match);

    IReadOnlyList<ModeStatistic> FindModeStatistics(byte gameModeId, IReadOnlyCollection<int> accountIds);

    void SaveFinalMove(MatchClosure closure);

    void SaveClosureWithoutMove(MatchClosure closure);

    IReadOnlyList<int> FindMatchIdsWithRunOutClock(DateTime now);
}
