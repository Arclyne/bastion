using System;
using System.Globalization;
using System.Threading;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Notifications;
using Bastion.Domain.Matches;
using Gum.Mvvm;

namespace Bastion.Client.Screens.Matches;

public sealed class VersusViewModel : ViewModel, IDisposable
{
    public const int CountdownSeconds = 3;

    private const int CountdownTickSeconds = 1;

    private static readonly TimeSpan _countdownTick = TimeSpan.FromSeconds(CountdownTickSeconds);

    private readonly SynchronizationContext? _uiContext;
    private ITimer? _countdownTimer;
    private int _secondsLeft;

    public VersusViewModel(JoinedMatch joinedMatch, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(joinedMatch);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _uiContext = SynchronizationContext.Current;
        FoundMatchParticipant own = joinedMatch.FindOwnParticipant();
        FoundMatchParticipant rival = joinedMatch.FindRivalParticipant();
        FoundMatchFormat format = joinedMatch.FoundMatch.Format;
        OwnNickname = own.Nickname;
        OwnEloText = MatchScreenText.FormatElo(own.EloRating);
        OwnPawnSymbol = Participation.GetPawnSymbol(own.TurnOrder);
        RivalNickname = rival.Nickname;
        RivalEloText = MatchScreenText.FormatElo(rival.EloRating);
        RivalPawnSymbol = Participation.GetPawnSymbol(rival.TurnOrder);
        ModeText = MatchScreenText.FormatMode(format.BoardSize, format.WallsPerPlayer);
        _secondsLeft = CountdownSeconds;
        CountdownText = FormatCountdown(_secondsLeft);
        _countdownTimer = timeProvider.CreateTimer(HandleCountdownTick, null, _countdownTick, _countdownTick);
    }

    public string OwnNickname { get; }

    public string OwnEloText { get; }

    public PawnSymbol OwnPawnSymbol { get; }

    public string RivalNickname { get; }

    public string RivalEloText { get; }

    public PawnSymbol RivalPawnSymbol { get; }

    public string ModeText { get; }

    public string CountdownText
    {
        get => Get<string>();
        private set => Set(value);
    }

    public event EventHandler? CountdownFinished;

    public void Dispose()
    {
        StopCountdown();
    }

    private static string FormatCountdown(int secondsLeft)
    {
        return string.Format(CultureInfo.CurrentCulture, Strings.VersusStartsInFormat, secondsLeft);
    }

    private void AdvanceCountdown()
    {
        if (_countdownTimer is null)
        {
            return;
        }

        _secondsLeft--;
        if (_secondsLeft > 0)
        {
            CountdownText = FormatCountdown(_secondsLeft);
            return;
        }

        StopCountdown();
        OnCountdownFinished();
    }

    private void StopCountdown()
    {
        Interlocked.Exchange(ref _countdownTimer, null)?.Dispose();
    }

    private void OnCountdownFinished()
    {
        CountdownFinished?.Invoke(this, EventArgs.Empty);
    }

    private void HandleCountdownTick(object? state)
    {
        UiThread.Run(_uiContext, AdvanceCountdown);
    }
}
