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
        return Environment.GetEnvironmentVariable(ThumbprintVariable)?.Trim() ?? string.Empty;
    }

    private static string GetHost()
    {
        string? host = Environment.GetEnvironmentVariable(HostVariable);
        return string.IsNullOrWhiteSpace(host) ? ServiceEndpoints.DefaultHost : host;
    }

    private static int GetPort()
    {
        string? text = Environment.GetEnvironmentVariable(PortVariable);
        bool isValid = int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out int port) && port > 0;
        return isValid ? port : ServiceEndpoints.DefaultPort;
    }
}
