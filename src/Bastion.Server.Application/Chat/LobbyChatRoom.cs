using System;
using System.Collections.Generic;
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

    public bool TryStartCooldown(int accountId, DateTime now)
    {
        return _cooldown.TryStartCooldown(accountId, now);
    }

    public PostedChatLine Post(ChatLine line)
    {
        ArgumentNullException.ThrowIfNull(line);

        PostedChatLine postedLine = Store(line);
        _broadcaster.Broadcast(postedLine);
        return postedLine;
    }

    public IReadOnlyList<PostedChatLine> ListRecent()
    {
        lock (_roomLock)
        {
            return new List<PostedChatLine>(_history);
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
            if (_history.Count > ChatPolicy.MaximumRecentMessages)
            {
                _history.Dequeue();
            }

            return postedLine;
        }
    }
}
