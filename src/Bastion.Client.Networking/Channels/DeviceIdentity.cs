using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Bastion.Client.Networking.Channels;

public static class DeviceIdentity
{
    private const string UnknownVersion = "0.0.0";
    private const char Separator = '|';
    private const int VersionFieldCount = 3;

    public static string GetFingerprint()
    {
        string source = Environment.MachineName + Separator + Environment.UserName;
        byte[] encodedSource = Encoding.UTF8.GetBytes(source);
        byte[] hash = SHA256.HashData(encodedSource);
        return Convert.ToHexString(hash);
    }

    public static string GetClientVersion()
    {
        Version? version = Assembly.GetEntryAssembly()?.GetName().Version;
        return version?.ToString(VersionFieldCount) ?? UnknownVersion;
    }
}
