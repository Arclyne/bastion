using System;
using System.Collections.Generic;
using System.ServiceModel;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Chat;
using Bastion.Contracts.Chat;
using Bastion.Contracts.PrivateChat;

namespace Bastion.Client.Networking.PrivateChat;

public sealed class ConversationClient : IConversationClient
{
    private readonly IServiceChannel<IConversationService> _channel;

    public ConversationClient(IServiceChannel<IConversationService> channel)
    {
        ArgumentNullException.ThrowIfNull(channel);

        _channel = channel;
    }

    public IReadOnlyList<ConversationPreview> ListConversations(string sessionToken)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        ConversationEntry[] entries = Call(service => service.ListConversations(sessionToken));
        return ConversationContractTranslator.ToPreviews(entries);
    }

    public ConversationTranscript OpenConversation(string sessionToken, string friendCode)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(friendCode);

        PrivateConversationLog log = Call(service => service.OpenConversation(sessionToken, friendCode));
        return ConversationContractTranslator.ToTranscript(log);
    }

    public DateTime SendMessage(string sessionToken, string friendCode, string text)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(friendCode);
        ArgumentNullException.ThrowIfNull(text);

        ChatMessageReceipt receipt = Call(service => service.SendPrivateMessage(sessionToken, friendCode, text));
        return receipt.SentAt;
    }

    public void MarkConversationRead(string sessionToken, string friendCode)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(friendCode);

        Call(service => service.MarkConversationRead(sessionToken, friendCode));
    }

    private TResult Call<TResult>(Func<IConversationService, TResult> operation)
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

    private static Exception ToClientException(Exception ex)
    {
        if (ex is FaultException<ChatMessageRejectedFault> rejected)
        {
            return ChatContractTranslator.ToMessageRejectedException(rejected);
        }

        return ServiceFailureTranslator.ToClientException(ex);
    }

    private void Call(Action<IConversationService> operation)
    {
        Func<IConversationService, bool> operationAsFunction = service => CompleteOperation(operation, service);
        Call(operationAsFunction);
    }

    private static bool CompleteOperation(Action<IConversationService> operation, IConversationService service)
    {
        operation(service);
        return true;
    }
}
