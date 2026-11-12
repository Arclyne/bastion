using System;
using System.Globalization;
using Bastion.Contracts;

namespace Bastion.Client.Networking.Channels;

public static class ServerAddress
{
    private const string HostVariable = "BASTION_SERVER_HOST";
    private const string PortVariable = "BASTION_SERVER_PORT";
    private const string ThumbprintVariable = "BASTION_SERVER_THUMBPRINT";

    public static Uri Create(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return ServiceEndpoints.CreateAddress(GetHost(), GetPort(), path);
    }

    public static string GetCertificateThumbprint()
    {
        string? thumbprint = Environment.GetEnvironmentVariable(ThumbprintVariable);
        return thumbprint?.Trim() ?? string.Empty;
    }

    private static string GetHost()
    {
        string? host = Environment.GetEnvironmentVariable(HostVariable);
        if (string.IsNullOrWhiteSpace(host))
        {
            return ServiceEndpoints.DefaultHost;
        }

        return host;
    }

    private static int GetPort()
    {
        string? portText = Environment.GetEnvironmentVariable(PortVariable);
        bool isNumber = int.TryParse(portText, NumberStyles.None, CultureInfo.InvariantCulture, out int port);
        if (!isNumber || !ServiceEndpoints.IsRegisteredPort(port))
        {
            return ServiceEndpoints.DefaultPort;
        }

        return port;
    }
}
