using System.Collections.Generic;
using Bastion.Server.Application.Chat;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed record NotifiedMatchChatMessage(
    int MatchId,
    IReadOnlyCollection<int> RecipientAccountIds,
    PostedChatLine PostedLine);
