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
        RemainingWalls = format.GameMode.WallsPerPlayer;
        RemainingClockMilliseconds = (int)TimeSpan.FromMinutes(format.ClockMinutes).TotalMilliseconds;
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

    public bool IsConnected { get; private set; }

    public static PawnSymbol GetPawnSymbol(byte turnOrder)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(turnOrder, Match.FirstTurnOrder);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(turnOrder, _pawnSymbolsInTurnOrder.Length);

        return _pawnSymbolsInTurnOrder[turnOrder - Match.FirstTurnOrder];
    }
}
