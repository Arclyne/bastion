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

    public event EventHandler<OpponentConnectionEventArgs>? OpponentDisconnected;

    public event EventHandler<OpponentConnectionEventArgs>? OpponentReconnected;

    public event EventHandler? MatchTakenOver;

    public event EventHandler<DrawEventArgs>? DrawOffered;

    public event EventHandler<DrawEventArgs>? DrawRejected;

    public event EventHandler<DrawEventArgs>? DrawOfferExpired;

    public Exception? DrawOfferFailure { get; set; }

    public int DrawOfferCount { get; private set; }

    public Exception? DrawAnswerFailure { get; set; }

    public bool? LastDrawAnswer { get; private set; }

    public FoundOngoingMatch OngoingMatch { get; set; } = FoundOngoingMatch.None;

    public Exception? OngoingMatchFailure { get; set; }

    public RejoinedMatch? Rejoined { get; set; }

    public Exception? RejoinFailure { get; set; }

    public int RejoinCount { get; private set; }

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

    public FoundOngoingMatch FindOngoingMatch(string sessionToken)
    {
        if (OngoingMatchFailure is not null)
        {
            throw OngoingMatchFailure;
        }

        return OngoingMatch;
    }

    public RejoinedMatch RejoinMatch(string sessionToken, int matchId)
    {
        RejoinCount++;
        if (RejoinFailure is not null)
        {
            throw RejoinFailure;
        }

        return Rejoined ?? throw new InvalidOperationException("The test set no rejoined match.");
    }

    public void OfferDraw(string sessionToken, int matchId)
    {
        if (DrawOfferFailure is not null)
        {
            throw DrawOfferFailure;
        }

        DrawOfferCount++;
    }

    public void RespondToDraw(string sessionToken, int matchId, bool isAccepted)
    {
        if (DrawAnswerFailure is not null)
        {
            throw DrawAnswerFailure;
        }

        LastDrawAnswer = isAccepted;
    }

    public void RaiseDrawOffered(DrawEventArgs e)
    {
        DrawOffered?.Invoke(this, e);
    }

    public void RaiseDrawRejected(DrawEventArgs e)
    {
        DrawRejected?.Invoke(this, e);
    }

    public void RaiseDrawOfferExpired(DrawEventArgs e)
    {
        DrawOfferExpired?.Invoke(this, e);
    }

    public void RaiseOpponentDisconnected(OpponentConnectionEventArgs e)
    {
        OpponentDisconnected?.Invoke(this, e);
    }

    public void RaiseOpponentReconnected(OpponentConnectionEventArgs e)
    {
        OpponentReconnected?.Invoke(this, e);
    }

    public void RaiseMatchTakenOver()
    {
        MatchTakenOver?.Invoke(this, EventArgs.Empty);
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
