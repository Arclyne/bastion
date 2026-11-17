using System;
using Bastion.Client.Networking.PrivateChat;

namespace Bastion.Client.Screens.MainMenu.Chat;

public sealed class PrivateMessageRowViewModel : MessageRowViewModel
{
    public PrivateMessageRowViewModel(PrivateMessage message, string dayLabel, TimeProvider timeProvider)
        : base(timeProvider)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(dayLabel);

        IsOwn = message.IsOwn;
        Text = message.Text;
        DayLabel = dayLabel;
        ShowSentTime(message.SentAt);
    }

    public string DayLabel { get; }

    public static PrivateMessageRowViewModel CreatePending(
        PrivateMessage message,
        string dayLabel,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(dayLabel);
        ArgumentNullException.ThrowIfNull(timeProvider);

        var row = new PrivateMessageRowViewModel(message, dayLabel, timeProvider);
        row.MarkPending();
        return row;
    }

    public bool HasDayLabel()
    {
        return DayLabel.Length > 0;
    }
}
