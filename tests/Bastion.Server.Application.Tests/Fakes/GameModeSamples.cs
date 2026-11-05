using Bastion.Domain.GameModes;

namespace Bastion.Server.Application.Tests.Fakes;

public static class GameModeSamples
{
    public const byte ClassicWallsPerPlayer = 10;

    private const byte ClassicGameModeId = 1;
    private const byte FourPlayersGameModeId = 2;
    private const byte BoardSize = 9;
    private const byte TwoPlayers = 2;
    private const byte FourPlayers = 4;
    private const byte FourPlayersWallsPerPlayer = 5;

    public static GameMode CreateClassic()
    {
        return new GameMode
        {
            GameModeId = ClassicGameModeId,
            Code = GameModeCode.Classic,
            BoardSize = BoardSize,
            PlayerCount = TwoPlayers,
            WallsPerPlayer = ClassicWallsPerPlayer,
            IsActive = true,
        };
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
}
