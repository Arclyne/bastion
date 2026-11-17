using System;
using Bastion.Client.Localization;

namespace Bastion.Client.Screens.MainMenu.Chat;

public sealed record ConversationPresence
{
    public ConversationPresence(PartnerPresence presence, string statusText)
    {
        ArgumentNullException.ThrowIfNull(statusText);

        Presence = presence;
        StatusText = statusText;
    }

    public PartnerPresence Presence { get; }

    public string StatusText { get; }

    public static ConversationPresence CreateForNonFriend()
    {
        return new ConversationPresence(PartnerPresence.NotFriend, Strings.PrivateStatusNotFriend);
    }
}
