using System;

namespace Bastion.Contracts;

public static class ServiceEndpoints
{
    public const string DefaultHost = "localhost";
    public const int DefaultPort = 8000;
    public const string AccountsPath = "accounts";

    private const string Scheme = "net.tcp";

    public static Uri CreateAddress(string host, int port, string path)
    {
        return new UriBuilder(Scheme, host, port, path).Uri;
    }
}
