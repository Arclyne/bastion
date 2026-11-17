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
            if (_lastMessageTimesByAccountId.TryGetValue(accountId, out DateTime lastMessageAt)
                && now < lastMessageAt.AddSeconds(ChatPolicy.MessageCooldownSeconds))
            {
                return false;
            }

            _lastMessageTimesByAccountId[accountId] = now;
            return true;
        }
    }
}
