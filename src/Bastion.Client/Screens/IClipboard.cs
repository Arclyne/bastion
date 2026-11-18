namespace Bastion.Client.Screens;

public interface IClipboard
{
    bool CanCopyText { get; }

    void CopyText(string text);
}
