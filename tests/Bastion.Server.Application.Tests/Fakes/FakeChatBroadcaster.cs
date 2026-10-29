using System.Collections.Generic;
using Bastion.Server.Application.Chat;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeChatBroadcaster : IChatBroadcaster
{
    public List<ChatLine> Lines { get; } = new List<ChatLine>();

    public void Broadcast(ChatLine line)
    {
        Lines.Add(line);
    }
}
