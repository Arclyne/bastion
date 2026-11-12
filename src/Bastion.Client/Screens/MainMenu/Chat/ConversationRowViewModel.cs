using System;
using System.Globalization;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Friends;
using Bastion.Client.Networking.PrivateChat;
using Gum.Mvvm;

namespace Bastion.Client.Screens.MainMenu.Chat;

public sealed class ConversationRowViewModel : ViewModel
{
    public ConversationRowViewModel(ConversationPreview preview, DateTime today)
    {
        ArgumentNullException.ThrowIfNull(preview);

        Partner = preview.Contact.Partner;
        IsFriend = preview.Contact.IsFriend;
        PreviewText = string.Empty;
        TimeText = string.Empty;
        UnreadText = string.Empty;
        ShowMessage(preview.LastMessage, preview.UnreadCount, today);
    }

    public PlayerTag Partner { get; }

    public bool IsFriend { get; }

    public int UnreadCount { get; private set; }

    public string PreviewText
    {
        get => Get<string>();
        private set => Set(value);
    }

    public string TimeText
    {
        get => Get<string>();
        private set => Set(value);
    }

    public string UnreadText
    {
        get => Get<string>();
        private set => Set(value);
    }

    [DependsOn(nameof(UnreadText))]
    public bool HasUnread => UnreadText.Length > 0;

    public void ShowMessage(PrivateMessage message, int unreadCount, DateTime today)
    {
        ArgumentNullException.ThrowIfNull(message);

        PreviewText = message.IsOwn
            ? string.Format(CultureInfo.CurrentCulture, Strings.ChatOwnPreviewFormat, message.Text)
            : message.Text;
        TimeText = ChatTimeText.DescribeLastActivity(message.SentAt, today);
        UpdateUnread(unreadCount);
    }

    public void UpdateUnread(int unreadCount)
    {
        UnreadCount = unreadCount;
        UnreadText = BadgeText.FormatCount(unreadCount);
    }
}
