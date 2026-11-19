using System;
using System.Collections.Generic;
using Bastion.Domain.Matches;

namespace Bastion.Server.Application.Matches;

public sealed record MatchSnapshot
{
    public required Match Match { get; init; }

    public required byte BoardSize { get; init; }

    public required IReadOnlyDictionary<int, string> NicknamesByAccountId { get; init; }

    public required DateTime TakenAt { get; init; }

    public required int RequestingAccountId { get; init; }

    public MatchEndNotice? Ending { get; init; }

    public ParticipationEndNotice? OwnDeparture { get; init; }
}
