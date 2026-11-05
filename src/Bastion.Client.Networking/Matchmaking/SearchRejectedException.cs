using System;

namespace Bastion.Client.Networking.Matchmaking;

public sealed class SearchRejectedException : Exception
{
    public SearchRejectedException(SearchRejection rejection, Exception innerException)
        : base($"The match search was rejected. Rejection={rejection}", innerException)
    {
        Rejection = rejection;
    }

    public SearchRejection Rejection { get; }
}
