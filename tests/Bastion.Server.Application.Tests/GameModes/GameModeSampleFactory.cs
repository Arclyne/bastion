using Bastion.Domain.GameModes;

namespace Bastion.Server.Application.Tests.GameModes;

public static class GameModeSampleFactory
{
    public const byte ClassicWallsPerPlayer = 10;

    private const byte ClassicGameModeId = 1;
    private const byte FourPlayersGameModeId = 2;
    private const byte BoardSize = 9;
    private const byte TwoPlayers = 2;
    private const byte FourPlayers = 4;
    private const byte FourPlayersWallsPerPlayer = 5;
    private const byte QuickGameModeId = 3;
    private const byte QuickBoardSize = 7;
    private const byte QuickWallsPerPlayer = 6;

    public static GameMode CreateClassic()
    {
        return CreateClassicWithSeats(TwoPlayers);
    }

    public static GameMode CreateClassicWithFourSeats()
    {
        return CreateClassicWithSeats(FourPlayers);
    }

    public static GameMode CreateFourPlayers()
    {
        return new GameMode
        {
            GameModeId = FourPlayersGameModeId,
            Code = GameModeCode.FourPlayers,
            BoardSize = BoardSize,
            PlayerCount = FourPlayers,
            WallsPerPlayer = FourPlayersWallsPerPlayer,
            IsActive = true,
        };
    }

    public static GameMode CreateQuick()
    {
        return new GameMode
        {
            GameModeId = QuickGameModeId,
            Code = GameModeCode.Quick,
            BoardSize = QuickBoardSize,
            PlayerCount = TwoPlayers,
            WallsPerPlayer = QuickWallsPerPlayer,
            IsActive = true,
        };
    }

    private static GameMode CreateClassicWithSeats(byte playerCount)
    {
        return new GameMode
        {
            GameModeId = ClassicGameModeId,
            Code = GameModeCode.Classic,
            BoardSize = BoardSize,
            PlayerCount = playerCount,
            WallsPerPlayer = ClassicWallsPerPlayer,
            IsActive = true,
        };
    }
}
