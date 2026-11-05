using System.Security.Cryptography;
using Bastion.Domain.Rooms;
using Bastion.Server.Application.Rooms;

namespace Bastion.Server.Adapters.Rooms;

public sealed class RoomCodeGenerator : IRoomCodeGenerator
{
    public string CreateCode()
    {
        return RandomNumberGenerator.GetString(RoomRules.CodeAlphabet, RoomRules.CodeLength);
    }
}
