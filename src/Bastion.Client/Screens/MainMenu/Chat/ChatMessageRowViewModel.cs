using System;
using System.Globalization;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Chat;
using Gum.Mvvm;

namespace Bastion.Client.Screens.MainMenu.Chat;

public sealed class ChatMessageRowViewModel : ViewModel
{
    private const string TimeFormat = "HH:mm";

    public ChatMessageRowViewModel(ChatMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        IsOwn = message.Author.IsOwn;
        AuthorText = IsOwn ? Strings.ChatOwnAuthor : message.Author.Nickname;
        Text = message.Text;
        TimeText = FormatTime(message.SentAt);
        StateText = string.Empty;
    }

    public bool IsOwn { get; }

    [DependsOn(nameof(IsOwn))]
    public bool IsOtherPlayer => !IsOwn;

    public string AuthorText { get; }

    public string Text { get; }

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

    public static ChatMessageRowViewModel CreatePending(string text, DateTime createdAt)
    {
        ArgumentNullException.ThrowIfNull(text);

        var row = new ChatMessageRowViewModel(new ChatMessage(new ChatAuthor(string.Empty, true), text, createdAt));
        row.StateText = Strings.ChatSending;
        row.IsPending = true;
        return row;
    }

    public void Confirm(DateTime sentAt)
    {
        TimeText = FormatTime(sentAt);
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

    private static string FormatTime(DateTime sentAt)
    {
        return sentAt.ToLocalTime().ToString(TimeFormat, CultureInfo.CurrentCulture);
    }
}
