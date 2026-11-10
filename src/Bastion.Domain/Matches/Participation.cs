using System;

namespace Bastion.Domain.Matches;

public class Participation
{
    private static readonly PawnSymbol[] _pawnSymbolsInTurnOrder = new PawnSymbol[]
    {
        PawnSymbol.Blue,
        PawnSymbol.Red,
        PawnSymbol.Green,
        PawnSymbol.Yellow,
    };

    internal Participation(MatchEntrant entrant, byte turnOrder, MatchFormat format)
    {
        AccountId = entrant.AccountId;
        TurnOrder = turnOrder;
        PawnSymbol = GetPawnSymbol(turnOrder);
        CurrentSquare = BoardLayout.GetStartingSquare(format.GameMode.BoardSize, turnOrder);
        RemainingWalls = format.WallsPerPlayer;
        var clockTime = TimeSpan.FromMinutes(format.ClockMinutes);
        double clockMilliseconds = clockTime.TotalMilliseconds;
        RemainingClockMilliseconds = (int)clockMilliseconds;
        InitialElo = entrant.EloRating;
        IsConnected = true;
    }

    private Participation()
    {
        CurrentSquare = string.Empty;
    }

    public int MatchId { get; private set; }

    public int AccountId { get; private set; }

    public byte TurnOrder { get; private set; }

    public PawnSymbol PawnSymbol { get; private set; }

    public string CurrentSquare { get; private set; }

    public byte RemainingWalls { get; private set; }

    public int? RemainingClockMilliseconds { get; private set; }

    public short? InitialElo { get; private set; }

    public ParticipationResult? Result { get; private set; }

    public byte? Placement { get; private set; }

    public MatchEndReason? EndReason { get; private set; }

    public bool IsConnected { get; private set; }

    public static PawnSymbol GetPawnSymbol(byte turnOrder)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(turnOrder, Match.FirstTurnOrder);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(turnOrder, _pawnSymbolsInTurnOrder.Length);

        int symbolIndex = turnOrder - Match.FirstTurnOrder;
        return _pawnSymbolsInTurnOrder[symbolIndex];
    }

    internal void Finish(ParticipationResult result)
    {
        if (Result.HasValue)
        {
            throw new InvalidOperationException("The participation has already ended.");
        }

        Result = result;
    }

    internal void Leave(MatchEndReason endReason, byte placement)
    {
        Finish(ParticipationResult.Lost);
        EndReason = endReason;
        Placement = placement;
    }

    internal void Win(byte placement)
    {
        Finish(ParticipationResult.Won);
        Placement = placement;
    }
}
