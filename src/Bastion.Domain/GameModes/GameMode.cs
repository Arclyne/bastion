namespace Bastion.Domain.GameModes;

public class GameMode
{
    public byte GameModeId { get; init; }

    public GameModeCode Code { get; init; }

    public byte BoardSize { get; init; }

    public byte PlayerCount { get; init; }

    public byte WallsPerPlayer { get; init; }

    public bool IsActive { get; init; }
}
