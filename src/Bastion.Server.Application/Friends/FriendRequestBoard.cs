using System;
using System.Collections.Generic;

namespace Bastion.Server.Application.Friends;

public sealed record FriendRequestBoard(
    IReadOnlyList<FriendRequestSummary> Received,
    IReadOnlyList<FriendRequestSummary> Sent,
    DateTime ListedAt);
