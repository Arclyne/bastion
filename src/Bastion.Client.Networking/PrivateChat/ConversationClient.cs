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

        Func<IConversationService, ConversationEntry[]?> listConversations = service => service.ListConversations(
            sessionToken);
        ConversationEntry[]? entries = Call(listConversations);
        return ConversationContractTranslator.ToPreviews(entries);
    }

    public ConversationTranscript OpenConversation(string sessionToken, string friendCode)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(friendCode);

        Func<IConversationService, PrivateConversationLog> openConversation = service => service.OpenConversation(
            sessionToken,
            friendCode);
        PrivateConversationLog log = Call(openConversation);
        return ConversationContractTranslator.ToTranscript(log);
    }

    public DateTime SendMessage(string sessionToken, string friendCode, string text)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(friendCode);
        ArgumentNullException.ThrowIfNull(text);

        Func<IConversationService, ChatMessageReceipt> sendMessage = service => service.SendPrivateMessage(
            sessionToken,
            friendCode,
            text);
        ChatMessageReceipt receipt = Call(sendMessage);
        return receipt.SentAt;
    }

    public void MarkConversationRead(string sessionToken, string friendCode)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(friendCode);

        Action<IConversationService> markConversationRead = service => service.MarkConversationRead(
            sessionToken,
            friendCode);
        Call(markConversationRead);
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

    private static Exception ToClientException(Exception cause)
    {
        if (cause is FaultException<ChatMessageRejectedFault> rejected)
        {
            return ChatContractTranslator.ToMessageRejectedException(rejected);
        }

        return ServiceFailureTranslator.ToClientException(cause);
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
