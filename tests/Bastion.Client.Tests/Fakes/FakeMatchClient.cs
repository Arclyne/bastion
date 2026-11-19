using System;
using Bastion.Client.Networking.Matches;
using Bastion.Client.Networking.Notifications;

namespace Bastion.Client.Tests.Fakes;

public sealed class FakeMatchClient : IMatchClient
{
    public Exception? AbandonFailure { get; set; }

    public Exception? PreviewFailure { get; set; }

    public PreviewedAbandonment Preview { get; set; } = new PreviewedAbandonment
    {
        IsRanked = false,
        InitialElo = null,
        FinalElo = null,
        Placement = null,
        ActivePlayerCount = 0,
    };

    public int? LastAbandonedMatchId { get; private set; }

    public Exception? MoveFailure { get; set; }

    public ChosenPawnMove? LastMove { get; private set; }

    public Exception? WallFailure { get; set; }

    public ChosenWall? LastWall { get; private set; }

    public int PlacedWallCount { get; private set; }

    public event EventHandler<ParticipationEndedEventArgs>? ParticipationEnded;

    public event EventHandler<MatchEndedEventArgs>? MatchEnded;

    public event EventHandler<PawnMovedEventArgs>? PawnMoved;

    public event EventHandler<WallPlacedEventArgs>? WallPlaced;

    public void AbandonMatch(string sessionToken, int matchId)
    {
        if (AbandonFailure is not null)
        {
            throw AbandonFailure;
        }

        LastAbandonedMatchId = matchId;
    }

    public PreviewedAbandonment PreviewAbandonment(string sessionToken, int matchId)
    {
        if (PreviewFailure is not null)
        {
            throw PreviewFailure;
        }

        return Preview;
    }

    public void MovePawn(string sessionToken, ChosenPawnMove chosenMove)
    {
        if (MoveFailure is not null)
        {
            throw MoveFailure;
        }

        LastMove = chosenMove;
    }

    public void PlaceWall(string sessionToken, ChosenWall wall)
    {
        if (WallFailure is not null)
        {
            throw WallFailure;
        }

        LastWall = wall;
        PlacedWallCount++;
    }

    public bool HasParticipationEndedSubscribers()
    {
        return ParticipationEnded is not null;
    }

    public bool HasMatchEndedSubscribers()
    {
        return MatchEnded is not null;
    }

    public bool HasPawnMovedSubscribers()
    {
        return PawnMoved is not null;
    }

    public void RaiseParticipationEnded(ParticipationEndedEventArgs e)
    {
        ParticipationEnded?.Invoke(this, e);
    }

    public void RaiseMatchEnded(MatchEndedEventArgs e)
    {
        MatchEnded?.Invoke(this, e);
    }

    public void RaisePawnMoved(PawnMovedEventArgs e)
    {
        PawnMoved?.Invoke(this, e);
    }

    public void RaiseWallPlaced(WallPlacedEventArgs e)
    {
        WallPlaced?.Invoke(this, e);
    }
}
