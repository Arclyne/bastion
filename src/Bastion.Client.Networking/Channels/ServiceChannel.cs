using System;
using System.ServiceModel;
using System.ServiceModel.Security;

namespace Bastion.Client.Networking.Channels;

public sealed class ServiceChannel<TContract> : IDisposable
    where TContract : class
{
    private const string ServerCertificateName = "bastion-server";
    private const int OpenTimeoutSeconds = 10;
    private const int SendTimeoutSeconds = 20;

    private readonly ChannelFactory<TContract> _factory;

    public ServiceChannel(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        var binding = new NetTcpBinding(SecurityMode.Transport)
        {
            OpenTimeout = TimeSpan.FromSeconds(OpenTimeoutSeconds),
            SendTimeout = TimeSpan.FromSeconds(SendTimeoutSeconds),
        };
        binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.None;

        var address = new EndpointAddress(ServerAddress.Create(path), new DnsEndpointIdentity(ServerCertificateName));
        _factory = new ChannelFactory<TContract>(binding, address);
        _factory.Credentials.ServiceCertificate.SslCertificateAuthentication = new X509ServiceCertificateAuthentication
        {
            CertificateValidationMode = X509CertificateValidationMode.Custom,
            CustomCertificateValidator = new ThumbprintCertificateValidator(ServerAddress.GetCertificateThumbprint()),
        };
    }

    public TResult Call<TResult>(Func<TContract, TResult> operation)
    {
        ArgumentNullException.ThrowIfNull(operation);

        TContract channel = _factory.CreateChannel();
        var communicationObject = (ICommunicationObject)channel;
        try
        {
            TResult result = operation(channel);
            communicationObject.Close();
            return result;
        }
        catch (CommunicationException)
        {
            communicationObject.Abort();
            throw;
        }
        catch (TimeoutException)
        {
            communicationObject.Abort();
            throw;
        }
    }

    public void Dispose()
    {
        if (_factory.State == CommunicationState.Faulted)
        {
            _factory.Abort();
            return;
        }

        _factory.Close();
    }
}
