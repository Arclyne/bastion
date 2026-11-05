using System;

namespace Bastion.Client.Networking.Notifications;

public sealed class MatchChatMessageReceivedEventArgs : EventArgs
{
    public MatchChatMessageReceivedEventArgs(int matchId, string senderNickname, string text)
    {
        ArgumentNullException.ThrowIfNull(senderNickname);
        ArgumentNullException.ThrowIfNull(text);

        MatchId = matchId;
        SenderNickname = senderNickname;
        Text = text;
    }

    public int MatchId { get; }

    public string SenderNickname { get; }

    public string Text { get; }
}
