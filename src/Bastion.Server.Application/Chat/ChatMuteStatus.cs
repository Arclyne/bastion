using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Moderation;

namespace Bastion.Server.Application.Chat;

public sealed record ChatMuteStatus(bool IsMuted, DateTime? EndsAt)
{
    public static ChatMuteStatus None { get; } = new ChatMuteStatus(IsMuted: false, EndsAt: null);

    public static ChatMuteStatus FromSanctionsInEffect(IReadOnlyCollection<Sanction> sanctions)
    {
        ArgumentNullException.ThrowIfNull(sanctions);

        if (sanctions.Count == 0)
        {
            return None;
        }

        if (sanctions.Any(sanction => sanction.IsPermanent()))
        {
            return new ChatMuteStatus(IsMuted: true, EndsAt: null);
        }

        DateTime? latestEnd = sanctions.Max(sanction => sanction.EndsAt);
        return new ChatMuteStatus(IsMuted: true, latestEnd);
    }
}
