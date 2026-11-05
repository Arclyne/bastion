using System;
using System.Collections.Generic;
using System.Linq;
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
        FoundMatchFormat format = joinedMatch.FoundMatch.Format;
        Own = ToVersusPlayer(joinedMatch.FindOwnParticipant());
        Rivals = joinedMatch.FoundMatch.Participants
            .Where(participant => !joinedMatch.IsOwnParticipant(participant))
            .OrderBy(participant => participant.TurnOrder)
            .Select(ToVersusPlayer)
            .ToList();
        ModeText = MatchScreenText.FormatMode(format.BoardSize, format.WallsPerPlayer);
        _secondsLeft = CountdownSeconds;
        CountdownText = FormatCountdown(_secondsLeft);
        _countdownTimer = timeProvider.CreateTimer(HandleCountdownTick, null, _countdownTick, _countdownTick);
    }

    public VersusPlayer Own { get; }

    public IReadOnlyList<VersusPlayer> Rivals { get; }

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

    private static VersusPlayer ToVersusPlayer(FoundMatchParticipant participant)
    {
        return new VersusPlayer(
            participant.Nickname,
            MatchScreenText.FormatElo(participant.EloRating),
            Participation.GetPawnSymbol(participant.TurnOrder));
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
