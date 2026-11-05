using System;

namespace Bastion.Server.Application.Chat;

public sealed class ChatChannelForbiddenException : Exception
{
    private const string DefaultMessage = "The player cannot write in this chat channel.";

    public ChatChannelForbiddenException()
        : base(DefaultMessage)
    {
    }

    public ChatChannelForbiddenException(string? message)
        : base(message)
    {
    }

    public ChatChannelForbiddenException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
