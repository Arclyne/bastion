using Bastion.Domain.GameModes;

namespace Bastion.Domain.Tests.GameModes;

public static class GameModeSampleFactory
{
    private const byte ClassicBoardSize = 9;
    private const byte QuickBoardSize = 7;
    private const byte TwoPlayers = 2;
    private const byte FourPlayers = 4;
    private const byte ClassicWallsPerPlayer = 10;
    private const byte QuickWallsPerPlayer = 6;
    private const byte FourPlayerWallsPerPlayer = 5;

    public static GameMode CreateClassic()
    {
        return Create(GameModeCode.Classic, ClassicBoardSize, ClassicWallsPerPlayer);
    }

    public static GameMode CreateQuick()
    {
        return Create(GameModeCode.Quick, QuickBoardSize, QuickWallsPerPlayer);
    }

    public static GameMode CreateFourPlayers()
    {
        return Create(GameModeCode.FourPlayers, ClassicBoardSize, FourPlayerWallsPerPlayer);
    }

    private static GameMode Create(GameModeCode code, byte boardSize, byte wallsPerPlayer)
    {
        byte playerCount = code == GameModeCode.FourPlayers ? FourPlayers : TwoPlayers;
        return new GameMode
        {
            Code = code,
            BoardSize = boardSize,
            PlayerCount = playerCount,
            WallsPerPlayer = wallsPerPlayer,
            IsActive = true,
        };
    }
}
