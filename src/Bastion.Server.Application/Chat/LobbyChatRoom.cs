using System;
using System.Collections.Generic;
using Bastion.Domain.Chat;

namespace Bastion.Server.Application.Chat;

public sealed class LobbyChatRoom : ILobbyChatRoom
{
    private readonly object _roomLock;
    private readonly Queue<PostedChatLine> _recentLines;
    private readonly ChatCooldown _cooldown;
    private readonly IChatBroadcaster _broadcaster;
    private long _lastMessageId;

    public LobbyChatRoom(IChatBroadcaster broadcaster)
    {
        ArgumentNullException.ThrowIfNull(broadcaster);

        _roomLock = new object();
        _recentLines = new Queue<PostedChatLine>();
        _cooldown = new ChatCooldown();
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
            return [.. _recentLines];
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
            _recentLines.Enqueue(postedLine);
            if (_recentLines.Count > ChatPolicy.MaximumRecentMessages)
            {
                _recentLines.Dequeue();
            }

            return postedLine;
        }
    }
}
