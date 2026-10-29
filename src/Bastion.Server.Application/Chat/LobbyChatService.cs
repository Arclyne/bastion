using System;
using Bastion.Domain.Chat;
using Bastion.Server.Application.Sessions;

namespace Bastion.Server.Application.Chat;

public sealed class LobbyChatService : ILobbyChatService
{
    private readonly ISessionVerifier _sessionVerifier;
    private readonly IChatBroadcaster _broadcaster;

    public LobbyChatService(ISessionVerifier sessionVerifier, IChatBroadcaster broadcaster)
    {
        ArgumentNullException.ThrowIfNull(sessionVerifier);
        ArgumentNullException.ThrowIfNull(broadcaster);

        _sessionVerifier = sessionVerifier;
        _broadcaster = broadcaster;
    }

    public void Send(string sessionToken, string text, DateTime now)
    {
        ConnectedPlayer player = _sessionVerifier.Verify(sessionToken, now);
        if (!ChatRules.IsMessage(text))
        {
            throw new InvalidChatMessageException();
        }

        _broadcaster.Broadcast(new ChatLine(player.Nickname, text.Trim(), now));
    }
}
