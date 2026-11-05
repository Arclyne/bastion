using System;
using System.ServiceModel;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Notifications;
using Bastion.Contracts;
using Bastion.Contracts.Chat;

namespace Bastion.Client.Networking.Chat;

public sealed class ChatClient : IChatClient, IDisposable
{
    private readonly ServiceChannel<IChatService> _channel =
        new ServiceChannel<IChatService>(ServiceEndpoints.ChatPath);
    private readonly INotificationClient _notificationClient;

    public ChatClient(INotificationClient notificationClient)
    {
        ArgumentNullException.ThrowIfNull(notificationClient);

        _notificationClient = notificationClient;
    }

    public event EventHandler<MatchChatMessageReceivedEventArgs>? MatchChatMessageReceived
    {
        add => _notificationClient.MatchChatMessageReceived += value;
        remove => _notificationClient.MatchChatMessageReceived -= value;
    }

    public DateTime SendMessage(string sessionToken, string text)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(text);

        ChatMessageReceipt receipt = Call(service => service.SendChatMessage(sessionToken, text));
        return receipt.SentAt;
    }

    public DateTime SendMatchChatMessage(string sessionToken, int matchId, string text)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(text);

        ChatMessageReceipt receipt = Call(service => service.SendMatchChatMessage(sessionToken, matchId, text));
        return receipt.SentAt;
    }

    public LobbyChatTranscript OpenLobbyChat(string sessionToken)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        LobbyChatLog log = Call(service => service.OpenLobbyChat(sessionToken));
        return ChatContractTranslator.ToTranscript(log);
    }

    public void Dispose()
    {
        _channel.Dispose();
    }

    private static Exception ToClientException(Exception ex)
    {
        if (ex is FaultException<ChatMessageRejectedFault> rejected)
        {
            return new ChatMessageRejectedException(
                ChatContractTranslator.ToRejection(rejected.Detail.Reason),
                rejected.Detail.MutedUntil,
                ex);
        }

        if (ex is FaultException<ChatChannelForbiddenFault>)
        {
            return new ChatChannelForbiddenException(ex);
        }

        return ServiceFailureTranslator.ToClientException(ex);
    }

    private TResult Call<TResult>(Func<IChatService, TResult> operation)
    {
        try
        {
            return _channel.Call(operation);
        }
        catch (CommunicationException ex)
        {
            throw ToClientException(ex);
        }
        catch (TimeoutException ex)
        {
            throw ToClientException(ex);
        }
    }
}
