using System;

namespace Bastion.Client.Screens.MainMenu.Friends;

public sealed class FriendRemovedEventArgs : EventArgs
{
    public FriendRemovedEventArgs(string friendCode)
    {
        ArgumentNullException.ThrowIfNull(friendCode);

        FriendCode = friendCode;
    }

    public string FriendCode { get; }
}
