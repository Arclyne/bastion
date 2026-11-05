using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Chat;

namespace Bastion.Server.Application.Chat;

public sealed class LobbyChatRoom : ILobbyChatRoom
{
    private readonly object _roomLock = new object();
    private readonly Queue<PostedChatLine> _history = new Queue<PostedChatLine>();
    private readonly Dictionary<int, DateTime> _lastTurnByAccountId = new Dictionary<int, DateTime>();
    private readonly IChatBroadcaster _broadcaster;
    private long _lastMessageId;

    public LobbyChatRoom(IChatBroadcaster broadcaster)
    {
        ArgumentNullException.ThrowIfNull(broadcaster);

        _broadcaster = broadcaster;
    }

    public bool TryClaimTurn(int accountId, DateTime now)
    {
        lock (_roomLock)
        {
            if (_lastTurnByAccountId.TryGetValue(accountId, out DateTime lastTurn)
                && now < lastTurn.AddSeconds(ChatRules.CooldownSeconds))
            {
                return false;
            }

            _lastTurnByAccountId[accountId] = now;
            return true;
        }
    }

    public PostedChatLine Post(ChatLine line)
    {
        ArgumentNullException.ThrowIfNull(line);

        PostedChatLine postedLine = Store(line);
        _broadcaster.Broadcast(postedLine);
        return postedLine;
    }

    public IReadOnlyList<PostedChatLine> ListRecent(int count)
    {
        lock (_roomLock)
        {
            return _history.TakeLast(count).ToList();
        }
    }

    public int CountListeners()
    {
        return _broadcaster.CountListeners();
    }

    private PostedChatLine Store(ChatLine line)
    {
        lock (_roomLock)
        {
            _lastMessageId++;
            var postedLine = new PostedChatLine(_lastMessageId, line);
            _history.Enqueue(postedLine);
            if (_history.Count > ChatRules.HistoryCapacity)
            {
                _history.Dequeue();
            }

            return postedLine;
        }
    }
}
