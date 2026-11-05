namespace Bastion.Domain.Moderation;

public class BannedWord
{
    private BannedWord()
    {
        Term = string.Empty;
    }

    public int BannedWordId { get; private set; }

    public string Term { get; private set; }

    public BannedWordScope Scope { get; private set; }
}
