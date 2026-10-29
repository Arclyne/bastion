using System;

namespace Bastion.Client.Networking.Chat;

public sealed class InvalidChatMessageException : Exception
{
    public InvalidChatMessageException(Exception innerException)
        : base("The chat message is empty or too long.", innerException)
    {
    }
}
