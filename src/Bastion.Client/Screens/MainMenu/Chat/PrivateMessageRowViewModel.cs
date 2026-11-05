using System;
using Bastion.Client.Localization;
using Bastion.Client.Networking.PrivateChat;
using Gum.Mvvm;

namespace Bastion.Client.Screens.MainMenu.Chat;

public sealed class PrivateMessageRowViewModel : ViewModel
{
    public PrivateMessageRowViewModel(PrivateMessage message, string dayLabel)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(dayLabel);

        IsOwn = message.IsOwn;
        Text = message.Text;
        SentAt = message.SentAt;
        DayLabel = dayLabel;
        TimeText = ChatTimeText.FormatTime(message.SentAt);
        StateText = string.Empty;
    }

    public bool IsOwn { get; }

    public string Text { get; }

    public DateTime SentAt { get; }

    public string DayLabel { get; }

    public bool HasDayLabel => DayLabel.Length > 0;

    public string TimeText
    {
        get => Get<string>();
        private set => Set(value);
    }

    public string StateText
    {
        get => Get<string>();
        private set => Set(value);
    }

    public bool IsPending
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public bool HasFailed
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public static PrivateMessageRowViewModel CreatePending(PrivateMessage message, string dayLabel)
    {
        var row = new PrivateMessageRowViewModel(message, dayLabel);
        row.StateText = Strings.ChatSending;
        row.IsPending = true;
        return row;
    }

    public void Confirm(DateTime sentAt)
    {
        TimeText = ChatTimeText.FormatTime(sentAt);
        StateText = string.Empty;
        IsPending = false;
    }

    public void Fail(string reason)
    {
        ArgumentNullException.ThrowIfNull(reason);

        StateText = reason;
        IsPending = false;
        HasFailed = true;
    }
}
