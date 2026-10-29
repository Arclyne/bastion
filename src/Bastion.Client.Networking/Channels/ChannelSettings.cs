using System;
using System.ServiceModel;
using System.ServiceModel.Security;

namespace Bastion.Client.Networking.Channels;

public static class ChannelSettings
{
    private const string ServerCertificateName = "bastion-server";
    private const int OpenTimeoutSeconds = 10;
    private const int SendTimeoutSeconds = 20;
    private const int ReceiveTimeoutMinutes = 30;

    public static NetTcpBinding CreateBinding()
    {
        var binding = new NetTcpBinding(SecurityMode.Transport)
        {
            OpenTimeout = TimeSpan.FromSeconds(OpenTimeoutSeconds),
            SendTimeout = TimeSpan.FromSeconds(SendTimeoutSeconds),
            ReceiveTimeout = TimeSpan.FromMinutes(ReceiveTimeoutMinutes),
        };
        binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.None;
        return binding;
    }

    public static EndpointAddress CreateAddress(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return new EndpointAddress(ServerAddress.Create(path), new DnsEndpointIdentity(ServerCertificateName));
    }

    public static void ApplyCertificateValidation(ChannelFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        factory.Credentials.ServiceCertificate.SslCertificateAuthentication = new X509ServiceCertificateAuthentication
        {
            CertificateValidationMode = X509CertificateValidationMode.Custom,
            CustomCertificateValidator = new ThumbprintCertificateValidator(ServerAddress.GetCertificateThumbprint()),
        };
    }
}
