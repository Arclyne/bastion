using System.Collections.Generic;
using Bastion.Domain.Chat;
using Bastion.Domain.GameModes;
using Bastion.Domain.Matches;

namespace Bastion.Server.Application.Matches;

public interface IMatchStore
{
    bool HasUnfinishedParticipation(int accountId);

    GameMode? FindActiveGameMode(GameModeCode code);

    IReadOnlyDictionary<int, short> FindEloRatingsByAccountId(byte gameModeId, IReadOnlyCollection<int> accountIds);

    int SaveMatch(Match match);

    IReadOnlyList<int> FindAccountIdsWithUnfinishedParticipation(int matchId);

    long SaveChatMessage(ChatMessage message);
}
