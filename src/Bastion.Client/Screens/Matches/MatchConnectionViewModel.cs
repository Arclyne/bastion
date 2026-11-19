using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Matches;
using Bastion.Client.Networking.Matchmaking;
using Bastion.Client.Networking.Notifications;
using Bastion.Client.Networking.Sessions;
using Bastion.Domain.Matches;
using Gum.Mvvm;
using log4net;

namespace Bastion.Client.Screens.Matches;

public sealed class MatchConnectionViewModel : ViewModel, IDisposable
{
    public const int ToastSeconds = 3;
    public const int ReturnNoticeSeconds = 6;
    public const int TakeOverGraceSeconds = 10;
    public const int EndingCountdownSeconds = 10;

    private const int RivalTickSeconds = 1;
    private const double FullPercent = 100;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(MatchConnectionViewModel));
    private static readonly int[] _retryDelaysInSeconds = [2, 5, 10, 20, 30];
    private static readonly TimeSpan _rivalTick = TimeSpan.FromSeconds(RivalTickSeconds);
    private static readonly TimeSpan _takeOverGrace = TimeSpan.FromSeconds(TakeOverGraceSeconds);
    private static readonly TimeSpan _endingCountdown = TimeSpan.FromSeconds(EndingCountdownSeconds);

    private readonly JoinedMatch _joinedMatch;
    private readonly MatchPlayViewModel _matchPlay;
    private readonly INotificationClient _notifications;
    private readonly IMatchClient _matches;
    private readonly TimeProvider _timeProvider;
    private readonly SynchronizationContext? _uiContext;
    private readonly Dictionary<byte, DateTimeOffset> _awaySinceByTurnOrder;
    private readonly byte _ownTurnOrder;
    private ITimer? _retryTimer;
    private ITimer? _rivalTimer;
    private ITimer? _toastTimer;
    private int _attemptNumber;
    private bool _wasOwnAway;
    private bool _isClosed;
    private DateTimeOffset _lastReconnectedAt;

    public MatchConnectionViewModel(
        JoinedMatch joinedMatch,
        MatchPlayViewModel matchPlayViewModel,
        MatchConnectionServices services)
    {
        ArgumentNullException.ThrowIfNull(joinedMatch);
        ArgumentNullException.ThrowIfNull(matchPlayViewModel);
        ArgumentNullException.ThrowIfNull(services);

        _joinedMatch = joinedMatch;
        _matchPlay = matchPlayViewModel;
        _notifications = services.Notifications;
        _matches = services.Matches;
        _timeProvider = services.TimeProvider;
        _uiContext = SynchronizationContext.Current;
        _awaySinceByTurnOrder = [];
        _ownTurnOrder = joinedMatch.GetOwnTurnOrder();
        _lastReconnectedAt = DateTimeOffset.MinValue;
        RivalTitleText = string.Empty;
        RivalDetailText = string.Empty;
        CountdownText = string.Empty;
        ToastText = string.Empty;
        _notifications.Disconnected += HandleDisconnected;
        _matches.OpponentDisconnected += HandleOpponentDisconnected;
        _matches.OpponentReconnected += HandleOpponentReconnected;
        _matches.MatchTakenOver += HandleMatchTakenOver;
        _matchPlay.MatchEndReceived += HandleMatchEndReceived;
        _matchPlay.OwnParticipationEnded += HandleOwnParticipationEnded;
    }

    public bool IsReconnecting
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public bool IsTakenOver
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public bool HasAwayRival
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public string RivalTitleText
    {
        get => Get<string>();
        private set => Set(value);
    }

    public string RivalDetailText
    {
        get => Get<string>();
        private set => Set(value);
    }

    public string CountdownText
    {
        get => Get<string>();
        private set => Set(value);
    }

    public bool IsCountdownEnding
    {
        get => Get<bool>();
        private set => Set(value);
    }

    [DependsOn(nameof(IsCountdownEnding))]
    public bool IsCountdownRunning => !IsCountdownEnding;

    public float RivalProgressPercent
    {
        get => Get<float>();
        private set => Set(value);
    }

    public string ToastText
    {
        get => Get<string>();
        private set => Set(value);
    }

    public ConnectionToastTone ToastTone
    {
        get => Get<ConnectionToastTone>();
        private set => Set(value);
    }

    [DependsOn(nameof(ToastText))]
    public bool HasToast => ToastText.Length > 0;

    [DependsOn(nameof(ToastText))]
    [DependsOn(nameof(ToastTone))]
    public bool HasSuccessToast => ToastText.Length > 0 && ToastTone == ConnectionToastTone.Success;

    [DependsOn(nameof(ToastText))]
    [DependsOn(nameof(ToastTone))]
    public bool HasInformationToast => ToastText.Length > 0 && ToastTone == ConnectionToastTone.Information;

    public event EventHandler? AwayPlayersChanged;

    public event EventHandler? ReturnToMenuRequested;

    public bool IsAway(byte turnOrder)
    {
        return _awaySinceByTurnOrder.ContainsKey(turnOrder);
    }

    public TimeSpan GetTimeAway(byte turnOrder)
    {
        if (!_awaySinceByTurnOrder.TryGetValue(turnOrder, out DateTimeOffset awaySince))
        {
            return TimeSpan.Zero;
        }

        return _timeProvider.GetUtcNow() - awaySince;
    }

    public bool WasAwayAtEnd(byte turnOrder)
    {
        if (turnOrder == _ownTurnOrder)
        {
            return _wasOwnAway;
        }

        return _awaySinceByTurnOrder.ContainsKey(turnOrder);
    }

    public void TrackRejoinedPlayers(RejoinedMatch rejoinedMatch)
    {
        ArgumentNullException.ThrowIfNull(rejoinedMatch);

        DateTimeOffset now = _timeProvider.GetUtcNow();
        _awaySinceByTurnOrder.Clear();
        IEnumerable<RejoinedPlayer> awayRivals = rejoinedMatch.Players.Where(
            player => player.TurnOrder != _ownTurnOrder && !player.IsConnected && !player.HasEnded);
        foreach (RejoinedPlayer awayRival in awayRivals)
        {
            _awaySinceByTurnOrder[awayRival.TurnOrder] = now - awayRival.TimeAway;
        }

        RefreshAwayRivals();
    }

    public Task ReconnectNow()
    {
        if (!IsReconnecting)
        {
            return Task.CompletedTask;
        }

        return AttemptReconnection();
    }

    public void GoToMenu()
    {
        IsTakenOver = false;
        StopRetrying();
        ReturnToMenuRequested?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        _isClosed = true;
        StopRetrying();
        StopRivalTimer();
        Interlocked.Exchange(ref _toastTimer, value: null)?.Dispose();
        _notifications.Disconnected -= HandleDisconnected;
        _matches.OpponentDisconnected -= HandleOpponentDisconnected;
        _matches.OpponentReconnected -= HandleOpponentReconnected;
        _matches.MatchTakenOver -= HandleMatchTakenOver;
        _matchPlay.MatchEndReceived -= HandleMatchEndReceived;
        _matchPlay.OwnParticipationEnded -= HandleOwnParticipationEnded;
    }

    private void RefreshAwayRivals()
    {
        AwayPlayersChanged?.Invoke(this, EventArgs.Empty);
        if (_awaySinceByTurnOrder.Count == 0 || _matchPlay.IsMatchOver)
        {
            HasAwayRival = false;
            StopRivalTimer();
            return;
        }

        KeyValuePair<byte, DateTimeOffset> longestAway = _awaySinceByTurnOrder.MinBy(awayRival => awayRival.Value);
        ShowAwayRival(longestAway.Key, longestAway.Value);
        StartRivalTimer();
    }

    private void StopRivalTimer()
    {
        Interlocked.Exchange(ref _rivalTimer, value: null)?.Dispose();
    }

    private void ShowAwayRival(byte turnOrder, DateTimeOffset awaySince)
    {
        string nickname = FindNickname(turnOrder);
        TimeSpan timeAway = _timeProvider.GetUtcNow() - awaySince;
        TimeSpan waitLeft = MatchConnectionPolicy.ReturnDelay - timeAway;
        TimeSpan shownWaitLeft = waitLeft > TimeSpan.Zero ? waitLeft : TimeSpan.Zero;
        bool hasFourPlayers = _joinedMatch.FoundMatch.Participants.Count > Match.TwoPlayerCount;
        double progressPercent = Math.Clamp(
            timeAway / MatchConnectionPolicy.ReturnDelay * FullPercent,
            0,
            FullPercent);
        float shownPercent = (float)progressPercent;
        RivalTitleText = string.Format(CultureInfo.CurrentCulture, Strings.RivalAwayTitleFormat, nickname);
        RivalDetailText = hasFourPlayers ? Strings.RivalAwayFourPlayersDetail : Strings.RivalAwayDetail;
        CountdownText = MatchScreenText.FormatClock(shownWaitLeft);
        IsCountdownEnding = shownWaitLeft <= _endingCountdown;
        RivalProgressPercent = shownPercent;
        HasAwayRival = true;
    }

    private string FindNickname(byte turnOrder)
    {
        FoundMatchParticipant? participant = _joinedMatch.FoundMatch.Participants.FirstOrDefault(
            candidate => candidate.TurnOrder == turnOrder);
        if (participant is null)
        {
            return string.Empty;
        }

        return participant.Nickname;
    }

    private void StartRivalTimer()
    {
        if (_rivalTimer is not null)
        {
            return;
        }

        _rivalTimer = _timeProvider.CreateTimer(HandleRivalTick, state: null, _rivalTick, _rivalTick);
    }

    private void HandleRivalTick(object? state)
    {
        UiThread.Run(_uiContext, RefreshAwayRivals);
    }

    private async Task AttemptReconnection()
    {
        StopRetrying();
        if (_isClosed)
        {
            return;
        }

        try
        {
            RejoinedMatch rejoinedMatch = await Task.Run(ReconnectAndRejoin);
            FinishReconnecting(rejoinedMatch);
        }
        catch (MatchNotJoinedException ex)
        {
            _logger.Warn("The server says the player never played the match being rejoined.", ex);
            GoToMenu();
        }
        catch (InvalidSessionException ex)
        {
            ReportFailedAttempt(ex);
        }
        catch (ServerUnreachableException ex)
        {
            ReportFailedAttempt(ex);
        }
        catch (ConnectionLostException ex)
        {
            ReportFailedAttempt(ex);
        }
        catch (ServerErrorException ex)
        {
            ReportFailedAttempt(ex);
        }
    }

    private void StopRetrying()
    {
        Interlocked.Exchange(ref _retryTimer, value: null)?.Dispose();
    }

    private RejoinedMatch ReconnectAndRejoin()
    {
        string token = _joinedMatch.Session.Token;
        _notifications.Connect(token);
        return _matches.RejoinMatch(token, _joinedMatch.FoundMatch.MatchId);
    }

    private void FinishReconnecting(RejoinedMatch rejoinedMatch)
    {
        if (_isClosed)
        {
            return;
        }

        _logger.Info($"The player is back in the match. MatchId={_joinedMatch.FoundMatch.MatchId}");
        _lastReconnectedAt = _timeProvider.GetUtcNow();
        IsReconnecting = false;
        TrackRejoinedPlayers(rejoinedMatch);
        int missedMoveCount = _matchPlay.ApplyRejoinedMatch(rejoinedMatch);
        if (_matchPlay.IsMatchOver)
        {
            return;
        }

        _wasOwnAway = false;
        string returnText = DescribeReturn(missedMoveCount);
        ShowToast(returnText, ConnectionToastTone.Information, ReturnNoticeSeconds);
    }

    private static string DescribeReturn(int missedMoveCount)
    {
        if (missedMoveCount == 0)
        {
            return Strings.MatchBackNoMoves;
        }

        if (missedMoveCount == 1)
        {
            return Strings.MatchBackOneMove;
        }

        return string.Format(CultureInfo.CurrentCulture, Strings.MatchBackMovesFormat, missedMoveCount);
    }

    private void ShowToast(string text, ConnectionToastTone tone, int seconds)
    {
        Interlocked.Exchange(ref _toastTimer, value: null)?.Dispose();
        ToastTone = tone;
        ToastText = text;
        var duration = TimeSpan.FromSeconds(seconds);
        _toastTimer = _timeProvider.CreateTimer(HandleToastTimer, state: null, duration, Timeout.InfiniteTimeSpan);
    }

    private void HandleToastTimer(object? state)
    {
        UiThread.Run(_uiContext, HideToast);
    }

    private void HideToast()
    {
        Interlocked.Exchange(ref _toastTimer, value: null)?.Dispose();
        ToastText = string.Empty;
    }

    private void ReportFailedAttempt(Exception cause)
    {
        _logger.Warn($"A reconnection attempt failed; it is tried again. Attempt={_attemptNumber}", cause);
        ScheduleAttempt();
    }

    private void ScheduleAttempt()
    {
        if (_isClosed)
        {
            return;
        }

        int delayIndex = Math.Min(_attemptNumber, _retryDelaysInSeconds.Length - 1);
        var delay = TimeSpan.FromSeconds(_retryDelaysInSeconds[delayIndex]);
        _attemptNumber++;
        _retryTimer = _timeProvider.CreateTimer(HandleRetryTimer, state: null, delay, Timeout.InfiniteTimeSpan);
    }

    private void HandleRetryTimer(object? state)
    {
        UiThread.Run(_uiContext, StartAttempt);
    }

    private void StartAttempt()
    {
        _ = AttemptReconnection();
    }

    private void HandleDisconnected(object? sender, EventArgs e)
    {
        UiThread.Run(_uiContext, StartReconnecting);
    }

    private void StartReconnecting()
    {
        if (_isClosed || IsReconnecting || _matchPlay.IsMatchOver)
        {
            return;
        }

        _logger.Warn($"The connection dropped during the match. MatchId={_joinedMatch.FoundMatch.MatchId}");
        _wasOwnAway = true;
        _attemptNumber = 0;
        IsReconnecting = true;
        _matchPlay.GoOffline();
        ScheduleAttempt();
    }

    private void HandleOpponentDisconnected(object? sender, OpponentConnectionEventArgs e)
    {
        if (!_joinedMatch.IsJoinedMatch(e.MatchId))
        {
            return;
        }

        Action markRivalAway = () => MarkRivalAway(e.TurnOrder);
        UiThread.Run(_uiContext, markRivalAway);
    }

    private void MarkRivalAway(byte turnOrder)
    {
        if (turnOrder == _ownTurnOrder || _awaySinceByTurnOrder.ContainsKey(turnOrder))
        {
            return;
        }

        _awaySinceByTurnOrder[turnOrder] = _timeProvider.GetUtcNow();
        RefreshAwayRivals();
    }

    private void HandleOpponentReconnected(object? sender, OpponentConnectionEventArgs e)
    {
        if (!_joinedMatch.IsJoinedMatch(e.MatchId))
        {
            return;
        }

        Action markRivalBack = () => MarkRivalBack(e.TurnOrder);
        UiThread.Run(_uiContext, markRivalBack);
    }

    private void MarkRivalBack(byte turnOrder)
    {
        if (!_awaySinceByTurnOrder.Remove(turnOrder))
        {
            return;
        }

        string nickname = FindNickname(turnOrder);
        string backText = string.Format(CultureInfo.CurrentCulture, Strings.RivalBackFormat, nickname);
        ShowToast(backText, ConnectionToastTone.Success, ToastSeconds);
        RefreshAwayRivals();
    }

    private void HandleMatchTakenOver(object? sender, EventArgs e)
    {
        UiThread.Run(_uiContext, ShowTakenOver);
    }

    private void ShowTakenOver()
    {
        TimeSpan sinceReconnected = _timeProvider.GetUtcNow() - _lastReconnectedAt;
        if (_isClosed || IsReconnecting || sinceReconnected < _takeOverGrace || _matchPlay.IsMatchOver)
        {
            return;
        }

        StopRetrying();
        _matchPlay.GoOffline();
        IsTakenOver = true;
    }

    private void HandleMatchEndReceived(object? sender, MatchEndedEventArgs e)
    {
        StopRetrying();
        RefreshAwayRivals();
    }

    private void HandleOwnParticipationEnded(object? sender, ParticipationEndedEventArgs e)
    {
        RefreshAwayRivals();
    }
}
