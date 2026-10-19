using System;
using System.Security.Cryptography.X509Certificates;
using CoreWCF;
using CoreWCF.Configuration;
using Microsoft.Extensions.Configuration;
using Bastion.Server.Host.Accounts;

namespace Bastion.Server.Host;

public sealed class ServerCertificate
{
    private const string PathKey = "Server:CertificatePath";
    private const string PasswordKey = "Server:CertificatePassword";

    private readonly X509Certificate2 _certificate;

    private ServerCertificate(X509Certificate2 certificate)
    {
        _certificate = certificate;
    }

    public static ServerCertificate Load(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        string? path = configuration[PathKey];
        string? password = configuration[PasswordKey];
        if (string.IsNullOrWhiteSpace(path) || password is null)
        {
            throw new InvalidOperationException($"The server certificate is not configured. Key={PathKey}");
        }

        return new ServerCertificate(X509CertificateLoader.LoadPkcs12FromFile(path, password));
    }

    public void ConfigureServices(IServiceBuilder serviceBuilder)
    {
        ArgumentNullException.ThrowIfNull(serviceBuilder);

        serviceBuilder.AddService<AccountService>();
        serviceBuilder.ConfigureAllServiceHostBase(ApplyTo);
    }

    private void ApplyTo(ServiceHostBase host)
    {
        host.Credentials.ServiceCertificate.Certificate = _certificate;
    }
}
