using System.Collections.Generic;
using Bastion.Server.Application.Rooms;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeRoomCodeGenerator : IRoomCodeGenerator
{
    public Queue<string> Codes { get; } = new Queue<string>();

    public string CreateCode()
    {
        return Codes.Dequeue();
    }
}
