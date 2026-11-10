using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Moderation;

namespace Bastion.Server.Application.Chat;

public sealed record ChatMute(bool IsMuted, DateTime? EndsAt)
{
    public static ChatMute None { get; } = new ChatMute(false, null);

    public static ChatMute FromSanctionsInEffect(IReadOnlyCollection<Sanction> sanctions)
    {
        ArgumentNullException.ThrowIfNull(sanctions);

        if (sanctions.Count == 0)
        {
            return None;
        }

        if (sanctions.Any(sanction => sanction.IsPermanent()))
        {
            return new ChatMute(true, null);
        }

        return new ChatMute(true, sanctions.Max(sanction => sanction.EndsAt));
    }
}
