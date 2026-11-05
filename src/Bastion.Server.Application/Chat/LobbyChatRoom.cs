using System;
using System.Collections.Generic;
using System.Linq;
using Bastion.Domain.Chat;

namespace Bastion.Server.Application.Chat;

public sealed class LobbyChatRoom : ILobbyChatRoom
{
    private readonly object _roomLock = new object();
    private readonly Queue<PostedChatLine> _history = new Queue<PostedChatLine>();
    private readonly ChatCooldown _cooldown = new ChatCooldown();
    private readonly IChatBroadcaster _broadcaster;
    private long _lastMessageId;

    public LobbyChatRoom(IChatBroadcaster broadcaster)
    {
        ArgumentNullException.ThrowIfNull(broadcaster);

        _broadcaster = broadcaster;
    }

    public bool TryClaimTurn(int accountId, DateTime now)
    {
        return _cooldown.TryClaimTurn(accountId, now);
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
