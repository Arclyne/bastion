using System;
using Bastion.Client.Localization;
using Gum.Mvvm;

namespace Bastion.Client.Screens.MainMenu.Chat;

public abstract class MessageRowViewModel : ViewModel
{
    private readonly TimeProvider _timeProvider;

    protected MessageRowViewModel(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        _timeProvider = timeProvider;
        Text = string.Empty;
        TimeText = string.Empty;
        StateText = string.Empty;
    }

    public bool IsOwn { get; protected init; }

    public string Text { get; protected init; }

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

    public void Confirm(DateTime sentAt)
    {
        ShowSentTime(sentAt);
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

    protected void ShowSentTime(DateTime sentAt)
    {
        TimeText = ChatTimeText.FormatTime(sentAt, _timeProvider);
    }

    protected void MarkPending()
    {
        StateText = Strings.ChatSending;
        IsPending = true;
    }
}
