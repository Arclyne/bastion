using System;

namespace Bastion.Server.Application.Chat;

public sealed class InvalidChatMessageException : Exception
{
    public InvalidChatMessageException()
        : base("The chat message is empty or too long.")
    {
    }
}
