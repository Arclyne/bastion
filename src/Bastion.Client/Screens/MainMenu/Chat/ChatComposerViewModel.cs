using System;
using System.Globalization;
using Bastion.Client.Localization;
using Bastion.Domain.Chat;
using Gum.Mvvm;

namespace Bastion.Client.Screens.MainMenu.Chat;

public abstract class ChatComposerViewModel : ViewModel
{
    private const int CounterVisibleFromLength = 180;

    private readonly TimeProvider _timeProvider;

    protected ChatComposerViewModel(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        _timeProvider = timeProvider;
        Draft = string.Empty;
        CounterText = string.Empty;
        StatusText = string.Empty;
    }

    public string Draft
    {
        get => Get<string>();
        set
        {
            if (!Set(value))
            {
                return;
            }

            StatusText = string.Empty;
            UpdateCounter();
        }
    }

    public string CounterText
    {
        get => Get<string>();
        private set => Set(value);
    }

    public bool IsCounterVisible
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public string StatusText
    {
        get => Get<string>();
        protected set => Set(value);
    }

    [DependsOn(nameof(StatusText))]
    public bool HasStatus => StatusText.Length > 0;

    public bool IsSending
    {
        get => Get<bool>();
        protected set => Set(value);
    }

    public abstract bool CanWrite { get; }

    public abstract bool CanSend { get; }

    protected static TimeProvider GetTimeProvider(ChatContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.TimeProvider;
    }

    protected static string FormatText(string format, params object[] arguments)
    {
        return string.Format(CultureInfo.CurrentCulture, format, arguments);
    }

    protected DateTime GetUtcNow()
    {
        DateTimeOffset now = _timeProvider.GetUtcNow();
        return now.UtcDateTime;
    }

    protected DateTime GetLocalToday()
    {
        return ChatTimeText.GetLocalToday(_timeProvider);
    }

    private void UpdateCounter()
    {
        int length = Draft.Length;
        IsCounterVisible = length >= CounterVisibleFromLength;
        CounterText = FormatText(Strings.ChatCounterFormat, length, ChatPolicy.MaximumMessageLength);
    }
}
