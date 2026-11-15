using System;
using System.ServiceModel;
using System.ServiceModel.Security;

namespace Bastion.Client.Networking.Channels;

public static class ChannelConfiguration
{
    private const string ServerCertificateName = "bastion-server";
    private const int OpenTimeoutSeconds = 10;
    private const int SendTimeoutSeconds = 20;
    private const int ReceiveTimeoutMinutes = 30;

    private static readonly TimeSpan _openTimeout = TimeSpan.FromSeconds(OpenTimeoutSeconds);
    private static readonly TimeSpan _sendTimeout = TimeSpan.FromSeconds(SendTimeoutSeconds);
    private static readonly TimeSpan _receiveTimeout = TimeSpan.FromMinutes(ReceiveTimeoutMinutes);

    public static NetTcpBinding CreateBinding()
    {
        var binding = new NetTcpBinding(SecurityMode.Transport)
        {
            OpenTimeout = _openTimeout,
            SendTimeout = _sendTimeout,
            ReceiveTimeout = _receiveTimeout,
        };
        binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.None;
        return binding;
    }

    public static EndpointAddress CreateAddress(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        Uri address = ServerAddress.Create(path);
        var identity = new DnsEndpointIdentity(ServerCertificateName);
        return new EndpointAddress(address, identity);
    }

    public static void ApplyCertificateValidation(ChannelFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        var certificateValidator = new ThumbprintX509CertificateValidator(ServerAddress.GetCertificateThumbprint());
        var certificateAuthentication = new X509ServiceCertificateAuthentication
        {
            CertificateValidationMode = X509CertificateValidationMode.Custom,
            CustomCertificateValidator = certificateValidator,
        };
        factory.Credentials.ServiceCertificate.SslCertificateAuthentication = certificateAuthentication;
    }
}
