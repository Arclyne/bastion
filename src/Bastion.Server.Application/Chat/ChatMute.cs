using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Moderation;

namespace Bastion.Server.Application.Chat;

public sealed record ChatMute(bool IsMuted, DateTime? EndsAt)
{
    public static ChatMute None { get; } = new ChatMute(IsMuted: false, EndsAt: null);

    public static ChatMute FromSanctionsInEffect(IReadOnlyCollection<Sanction> sanctions)
    {
        ArgumentNullException.ThrowIfNull(sanctions);

        if (sanctions.Count == 0)
        {
            return None;
        }

        if (sanctions.Any(sanction => sanction.IsPermanent()))
        {
            return new ChatMute(IsMuted: true, EndsAt: null);
        }

        DateTime? latestEnd = sanctions.Max(sanction => sanction.EndsAt);
        return new ChatMute(IsMuted: true, latestEnd);
    }
}
