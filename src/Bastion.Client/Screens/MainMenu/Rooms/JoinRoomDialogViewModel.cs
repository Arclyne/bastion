using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Rooms;
using Bastion.Domain.Rooms;
using Gum.Mvvm;

namespace Bastion.Client.Screens.MainMenu.Rooms;

public sealed class JoinRoomDialogViewModel : ViewModel, IDisposable
{
    private static readonly TimeSpan _tick = TimeSpan.FromSeconds(1);

    private readonly TimeProvider _timeProvider;
    private readonly SynchronizationContext? _uiContext;
    private ITimer? _retryTimer;
    private DateTimeOffset _retryAt;

    public JoinRoomDialogViewModel(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        _timeProvider = timeProvider;
        _uiContext = SynchronizationContext.Current;
        CodeText = string.Empty;
        ErrorText = string.Empty;
    }

    public bool IsOpen
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public string CodeText
    {
        get => Get<string>();
        set => Set(NormalizeCode(value));
    }

    public string ErrorText
    {
        get => Get<string>();
        private set => Set(value);
    }

    public bool IsSending
    {
        get => Get<bool>();
        set => Set(value);
    }

    public bool IsWaitingRetry
    {
        get => Get<bool>();
        private set => Set(value);
    }

    [DependsOn(nameof(ErrorText))]
    public bool HasError => ErrorText.Length > 0;

    [DependsOn(nameof(ErrorText))]
    public bool IsHintShown => !HasError;

    [DependsOn(nameof(IsSending))]
    public string EnterKey => IsSending ? nameof(Strings.RoomJoinEntering) : nameof(Strings.RoomJoinEnter);

    [DependsOn(nameof(CodeText))]
    [DependsOn(nameof(IsSending))]
    [DependsOn(nameof(IsWaitingRetry))]
    public bool CanEnter => CodeText.Length == RoomPolicy.CodeLength && !IsSending && !IsWaitingRetry;

    public void Open()
    {
        CodeText = string.Empty;
        if (!IsWaitingRetry)
        {
            ErrorText = string.Empty;
        }

        IsSending = false;
        IsOpen = true;
    }

    public void Close()
    {
        IsOpen = false;
    }

    public bool AcceptCode()
    {
        if (RoomPolicy.IsValidCode(CodeText))
        {
            ErrorText = string.Empty;
            return true;
        }

        ErrorText = Strings.RoomJoinInvalid;
        return false;
    }

    public void ShowRejection(RoomRejection rejection, TimeSpan retryDelay)
    {
        if (rejection == RoomRejection.TooManyAttempts)
        {
            StartRetryCountdown(retryDelay);
            return;
        }

        ErrorText = RoomRejectionText.Describe(rejection);
    }

    public void ShowError(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        ErrorText = text;
    }

    public void Dispose()
    {
        Interlocked.Exchange(ref _retryTimer, value: null)?.Dispose();
    }

    private static string NormalizeCode(string? code)
    {
        if (code is null)
        {
            return string.Empty;
        }

        string letters = new string(code.Where(char.IsLetterOrDigit).ToArray());
        string upperLetters = letters.ToUpper(CultureInfo.InvariantCulture);
        return upperLetters.Length > RoomPolicy.CodeLength ? upperLetters[..RoomPolicy.CodeLength] : upperLetters;
    }

    private void StartRetryCountdown(TimeSpan retryDelay)
    {
        Interlocked.Exchange(ref _retryTimer, value: null)?.Dispose();
        _retryAt = _timeProvider.GetUtcNow() + retryDelay;
        IsWaitingRetry = true;
        ShowRetryTime();
        _retryTimer = _timeProvider.CreateTimer(HandleRetryTick, state: null, _tick, _tick);
    }

    private void ShowRetryTime()
    {
        TimeSpan left = _retryAt - _timeProvider.GetUtcNow();
        if (left <= TimeSpan.Zero)
        {
            Interlocked.Exchange(ref _retryTimer, value: null)?.Dispose();
            IsWaitingRetry = false;
            ErrorText = string.Empty;
            return;
        }

        var roundedLeft = TimeSpan.FromSeconds(Math.Ceiling(left.TotalSeconds));
        ErrorText = RoomRejectionText.DescribeRetryDelay(roundedLeft);
    }

    private void HandleRetryTick(object? state)
    {
        UiThread.Run(_uiContext, ShowRetryTime);
    }
}
