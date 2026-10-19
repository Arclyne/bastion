using System;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Bastion.Client.Networking.Channels;

public sealed class ThumbprintCertificateValidator : X509CertificateValidator
{
    private readonly string _expectedThumbprint;

    public ThumbprintCertificateValidator(string expectedThumbprint)
    {
        ArgumentNullException.ThrowIfNull(expectedThumbprint);

        _expectedThumbprint = expectedThumbprint;
    }

    public override void Validate(X509Certificate2 certificate)
    {
        ArgumentNullException.ThrowIfNull(certificate);

        string thumbprint = certificate.GetCertHashString(HashAlgorithmName.SHA256);
        bool isTrusted = _expectedThumbprint.Length > 0
            && string.Equals(thumbprint, _expectedThumbprint, StringComparison.OrdinalIgnoreCase);
        if (!isTrusted)
        {
            throw new SecurityTokenValidationException(
                $"The server certificate is not trusted. Thumbprint={thumbprint}");
        }
    }
}
