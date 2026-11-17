using System;

namespace Bastion.Server.Application.Matchmaking;

public interface IRankedMatchCreator
{
    void CreateMatch(MatchPairing pairing, DateTime startedAt);
}
