using System;
using Bastion.Client.Networking.Matches;
using Bastion.Client.Networking.Notifications;

namespace Bastion.Client.Tests.Fakes;

public sealed class FakeMatchClient : IMatchClient
{
    public Exception? AbandonFailure { get; set; }

    public int? LastAbandonedMatchId { get; private set; }

    public Exception? MoveFailure { get; set; }

    public ChosenPawnMove? LastMove { get; private set; }

    public event EventHandler<MatchAbandonedEventArgs>? MatchAbandoned;

    public event EventHandler<PawnMovedEventArgs>? PawnMoved;

    public void AbandonMatch(string sessionToken, int matchId)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);

        if (AbandonFailure is not null)
        {
            throw AbandonFailure;
        }

        LastAbandonedMatchId = matchId;
    }

    public void MovePawn(string sessionToken, ChosenPawnMove move)
    {
        ArgumentNullException.ThrowIfNull(sessionToken);
        ArgumentNullException.ThrowIfNull(move);

        if (MoveFailure is not null)
        {
            throw MoveFailure;
        }

        LastMove = move;
    }

    public bool HasMatchAbandonedSubscribers()
    {
        return MatchAbandoned is not null;
    }

    public bool HasPawnMovedSubscribers()
    {
        return PawnMoved is not null;
    }

    public void RaiseMatchAbandoned(int matchId, string abandoningNickname, bool isMatchOver)
    {
        ArgumentNullException.ThrowIfNull(abandoningNickname);

        var abandonment = new MatchAbandonedEventArgs(matchId, abandoningNickname, isMatchOver);
        MatchAbandoned?.Invoke(this, abandonment);
    }

    public void RaisePawnMoved(PawnMovedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        PawnMoved?.Invoke(this, e);
    }
}
