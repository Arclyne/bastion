using System;

namespace Bastion.Server.Application.PrivateChat;

public sealed record PrivateMessageDraft
{
    public PrivateMessageDraft(string friendCode, string text)
    {
        ArgumentNullException.ThrowIfNull(friendCode);
        ArgumentNullException.ThrowIfNull(text);

        FriendCode = friendCode;
        Text = text;
    }

    public string FriendCode { get; }

    public string Text { get; }
}
