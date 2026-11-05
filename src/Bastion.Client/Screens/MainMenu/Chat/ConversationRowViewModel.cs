using System;
using System.Globalization;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Friends;
using Bastion.Client.Networking.PrivateChat;
using Gum.Mvvm;

namespace Bastion.Client.Screens.MainMenu.Chat;

public sealed class ConversationRowViewModel : ViewModel
{
    public ConversationRowViewModel(ConversationPreview preview)
    {
        ArgumentNullException.ThrowIfNull(preview);

        Partner = preview.Contact.Partner;
        IsFriend = preview.Contact.IsFriend;
        PreviewText = string.Empty;
        TimeText = string.Empty;
        UnreadText = string.Empty;
        ShowMessage(preview.LastMessage, preview.UnreadCount);
    }

    public PlayerTag Partner { get; }

    public string Nickname => Partner.Nickname;

    public bool IsFriend { get; }

    public DateTime LastSentAt { get; private set; }

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

    public void ShowMessage(PrivateMessage message, int unreadCount)
    {
        ArgumentNullException.ThrowIfNull(message);

        LastSentAt = message.SentAt;
        PreviewText = message.IsOwn
            ? string.Format(CultureInfo.CurrentCulture, Strings.ChatOwnPreviewFormat, message.Text)
            : message.Text;
        TimeText = ChatTimeText.DescribeLastActivity(message.SentAt, DateTime.Now.Date);
        UpdateUnread(unreadCount);
    }

    public void UpdateUnread(int unreadCount)
    {
        UnreadCount = unreadCount;
        UnreadText = unreadCount > 0 ? unreadCount.ToString(CultureInfo.CurrentCulture) : string.Empty;
    }
}
