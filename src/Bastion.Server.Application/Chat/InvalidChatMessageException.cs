using System;

namespace Bastion.Server.Application.Chat;

public sealed class InvalidChatMessageException : Exception
{
    private const string DefaultMessage = "The chat message is empty or too long.";

    public InvalidChatMessageException()
        : base(DefaultMessage)
    {
    }

    public InvalidChatMessageException(string? message)
        : base(message)
    {
    }

    public InvalidChatMessageException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
