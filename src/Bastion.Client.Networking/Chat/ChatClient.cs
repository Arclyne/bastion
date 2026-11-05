using System;
using System.ServiceModel;
using Bastion.Client.Networking.Channels;
using Bastion.Contracts;
using Bastion.Contracts.Chat;

namespace Bastion.Client.Networking.Chat;

public sealed class ChatClient : IChatClient, IDisposable
{
    private readonly ServiceChannel<IChatService> _channel =
        new ServiceChannel<IChatService>(ServiceEndpoints.ChatPath);

    public DateTime SendMessage(string sessionToken, string text)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(text);

        ChatMessageReceipt receipt = Call(service => service.SendChatMessage(sessionToken, text));
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
