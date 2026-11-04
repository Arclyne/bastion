using System;

namespace Bastion.Server.Application.Matchmaking;

public sealed class SearchRejectedException : Exception
{
    public SearchRejectedException(SearchRejection rejection)
        : base($"The match search was rejected. Rejection={rejection}")
    {
        Rejection = rejection;
    }

    public SearchRejection Rejection { get; }
}
