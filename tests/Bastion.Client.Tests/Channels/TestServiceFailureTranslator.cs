using System;
using System.ServiceModel;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Sessions;
using Bastion.Contracts.Sessions;
using Xunit;

namespace Bastion.Client.Tests.Channels;

public sealed class TestServiceFailureTranslator
{
    private const string FailureMessage = "The service could not be reached.";

    [Fact]
    public void ToClientException_EndpointNotFound_ReturnsServerUnreachableException()
    {
        var failure = new EndpointNotFoundException(FailureMessage);

        Exception translated = ServiceFailureTranslator.ToClientException(failure);

        Assert.IsType<ServerUnreachableException>(translated);
    }

    [Fact]
    public void ToClientException_InvalidSessionFault_ReturnsInvalidSessionException()
    {
        var fault = new InvalidSessionFault();
        var failure = new FaultException<InvalidSessionFault>(fault);

        Exception translated = ServiceFailureTranslator.ToClientException(failure);

        Assert.IsType<InvalidSessionException>(translated);
    }

    [Fact]
    public void ToClientException_UntypedFault_ReturnsServerErrorException()
    {
        var failure = new FaultException();

        Exception translated = ServiceFailureTranslator.ToClientException(failure);

        Assert.IsType<ServerErrorException>(translated);
    }

    [Fact]
    public void ToClientException_Timeout_ReturnsConnectionLostException()
    {
        var failure = new TimeoutException();

        Exception translated = ServiceFailureTranslator.ToClientException(failure);

        Assert.IsType<ConnectionLostException>(translated);
    }
}
