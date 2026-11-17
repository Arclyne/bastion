using System.Collections.Generic;
using Bastion.Server.Application.Rooms;

namespace Bastion.Server.Application.Tests.Fakes;

public sealed class FakeRoomCodeGenerator : IRoomCodeGenerator
{
    public FakeRoomCodeGenerator()
    {
        Codes = new Queue<string>();
    }

    public Queue<string> Codes { get; }

    public string CreateCode()
    {
        return Codes.Dequeue();
    }
}
