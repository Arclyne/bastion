using System;
using System.Collections.Generic;
using Bastion.Domain.Chat;

namespace Bastion.Server.Application.Chat;

public sealed class ChatCooldown
{
    private readonly object _cooldownLock = new object();
    private readonly Dictionary<int, DateTime> _lastMessageTimesByAccountId = new Dictionary<int, DateTime>();

    public bool TryStartCooldown(int accountId, DateTime now)
    {
        lock (_cooldownLock)
        {
            if (IsCoolingDown(accountId, now))
            {
                return false;
            }

            _lastMessageTimesByAccountId[accountId] = now;
            return true;
        }
    }

    private bool IsCoolingDown(int accountId, DateTime now)
    {
        bool hasSentMessage = _lastMessageTimesByAccountId.TryGetValue(accountId, out DateTime lastMessageAt);
        if (!hasSentMessage)
        {
            return false;
        }

        DateTime cooldownEnd = lastMessageAt.AddSeconds(ChatPolicy.MessageCooldownSeconds);
        return now < cooldownEnd;
    }
}
