using System;
using System.Collections.Generic;
using Bastion.Domain.Chat;

namespace Bastion.Server.Application.Chat;

public sealed class ChatCooldown
{
    private readonly object _turnsLock = new object();
    private readonly Dictionary<int, DateTime> _lastTurnByAccountId = new Dictionary<int, DateTime>();

    public bool TryClaimTurn(int accountId, DateTime now)
    {
        lock (_turnsLock)
        {
            if (_lastTurnByAccountId.TryGetValue(accountId, out DateTime lastTurn)
                && now < lastTurn.AddSeconds(ChatPolicy.MessageCooldownSeconds))
            {
                return false;
            }

            _lastTurnByAccountId[accountId] = now;
            return true;
        }
    }
}
