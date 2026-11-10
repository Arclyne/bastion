using System;

namespace Bastion.Server.Application.Chat;

public sealed record PostedChatLine
{
    public PostedChatLine(long messageId, ChatLine line)
    {
        ArgumentNullException.ThrowIfNull(line);

        MessageId = messageId;
        Line = line;
    }

    public long MessageId { get; }

    public ChatLine Line { get; }
}
