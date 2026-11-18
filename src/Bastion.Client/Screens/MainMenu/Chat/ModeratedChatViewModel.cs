using System;
using Bastion.Client.Networking.Chat;

namespace Bastion.Client.Screens.MainMenu.Chat;

public abstract class ModeratedChatViewModel : ChatComposerViewModel
{
    protected ModeratedChatViewModel(TimeProvider timeProvider)
        : base(timeProvider)
    {
        MuteText = string.Empty;
    }

    public bool IsMuted
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public string MuteText
    {
        get => Get<string>();
        private set => Set(value);
    }

    protected void ShowMuteStatus(ChatMuteStatus muteStatus)
    {
        ArgumentNullException.ThrowIfNull(muteStatus);

        IsMuted = muteStatus.IsMuted;
        MuteText = ChatRejectionText.DescribeMuteStatus(muteStatus, TimeProvider);
    }
}
