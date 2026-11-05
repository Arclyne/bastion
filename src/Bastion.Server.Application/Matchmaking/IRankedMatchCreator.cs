using System;
using System.Diagnostics.CodeAnalysis;

namespace Bastion.Server.Application.Matchmaking;

public interface IRankedMatchCreator
{
    bool TryCreateMatch(
        MatchPairing pairing,
        DateTime startedAt,
        [NotNullWhen(true)]
        out CreatedMatch? createdMatch);
}
