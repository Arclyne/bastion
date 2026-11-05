using System.Collections.Generic;
using Bastion.Server.Application.Chat;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeChatBroadcaster : IChatBroadcaster
{
    public List<PostedChatLine> Lines { get; } = new List<PostedChatLine>();

    public int ListenerCount { get; set; }

    public void Broadcast(PostedChatLine line)
    {
        Lines.Add(line);
    }

    public int CountListeners()
    {
        return ListenerCount;
    }
}
