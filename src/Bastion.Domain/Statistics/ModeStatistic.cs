namespace Bastion.Domain.Statistics;

public class ModeStatistic
{
    public const short DefaultEloRating = 1000;

    private ModeStatistic()
    {
    }

    public int AccountId { get; private set; }

    public byte GameModeId { get; private set; }

    public short EloRating { get; private set; }
}
