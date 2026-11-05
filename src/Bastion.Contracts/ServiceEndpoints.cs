using System;

namespace Bastion.Contracts;

public static class ServiceEndpoints
{
    public const string DefaultHost = "localhost";
    public const int DefaultPort = 8000;
    public const string AccountsPath = "accounts";
    public const string SessionsPath = "sessions";
    public const string ChatPath = "chat";
    public const string MatchmakingPath = "matchmaking";
    public const string MatchesPath = "matches";
    public const string FriendsPath = "friends";
    public const string ConversationsPath = "conversations";

    private const string Scheme = "net.tcp";

    public static Uri CreateAddress(string host, int port, string path)
    {
        return new UriBuilder(Scheme, host, port, path).Uri;
    }
}
