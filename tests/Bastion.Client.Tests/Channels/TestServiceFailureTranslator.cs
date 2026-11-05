using System;
using System.ServiceModel;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Sessions;
using Bastion.Contracts.Sessions;
using Xunit;

namespace Bastion.Client.Tests.Channels;

public class TestServiceFailureTranslator
{
    private const string FailureMessage = "The service could not be reached.";

    [Fact]
    public void ToClientException_EndpointNotFound_ReturnsServerUnreachable()
    {
        var failure = new EndpointNotFoundException(FailureMessage);

        Exception translated = ServiceFailureTranslator.ToClientException(failure);

        Assert.IsType<ServerUnreachableException>(translated);
    }

    [Fact]
    public void ToClientException_InvalidSessionFault_ReturnsInvalidSession()
    {
        var failure = new FaultException<InvalidSessionFault>(new InvalidSessionFault());

        Exception translated = ServiceFailureTranslator.ToClientException(failure);

        Assert.IsType<InvalidSessionException>(translated);
    }

    [Fact]
    public void ToClientException_UntypedFault_ReturnsServerError()
    {
        var failure = new FaultException();

        Exception translated = ServiceFailureTranslator.ToClientException(failure);

        Assert.IsType<ServerErrorException>(translated);
    }

    [Fact]
    public void ToClientException_Timeout_ReturnsConnectionLost()
    {
        var failure = new TimeoutException();

        Exception translated = ServiceFailureTranslator.ToClientException(failure);

        Assert.IsType<ConnectionLostException>(translated);
    }
}
