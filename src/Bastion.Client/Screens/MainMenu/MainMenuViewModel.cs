using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Matches;
using Bastion.Client.Networking.Notifications;
using Bastion.Client.Networking.Sessions;
using Bastion.Client.Screens.MainMenu.Rooms;
using Gum.Mvvm;
using log4net;

namespace Bastion.Client.Screens.MainMenu;

public sealed class MainMenuViewModel : ViewModel, IDisposable
{
    private const string RivalSeparator = ", ";

    private static readonly ILog _logger = LogManager.GetLogger(typeof(MainMenuViewModel));
    private static readonly int[] _retryDelaysInSeconds = [2, 5, 10, 20, 30];

    private readonly LoggedInSession _session;
    private readonly INotificationClient _notifications;
    private readonly IMatchClient _matches;
    private readonly TimeProvider _timeProvider;
    private readonly SynchronizationContext? _uiContext;
    private ITimer? _retryTimer;
    private int _attemptNumber;
    private bool _isClosed;

    public MainMenuViewModel(LoggedInSession session, MainMenuServices services)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(services);

        _session = session;
        _notifications = services.Notifications;
        _matches = services.Matches;
        _timeProvider = services.TimeProvider;
        _uiContext = SynchronizationContext.Current;
        Nickname = session.Nickname;
        ReturningDetailText = string.Empty;
        _notifications.Disconnected += HandleDisconnected;
    }

    public string Nickname { get; }

    public required PrivateRoomViewModel PrivateRoom { get; init; }

    public bool IsReturningToMatch
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public string ReturningDetailText
    {
        get => Get<string>();
        private set => Set(value);
    }

    public bool IsOffline
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public event EventHandler<MatchResumedEventArgs>? MatchResumed;

    public async Task ConnectNotifications()
    {
        try
        {
            await Task.Run(() => _notifications.Connect(_session.Token));
            _attemptNumber = 0;
            IsOffline = false;
        }
        catch (InvalidSessionException ex)
        {
            _logger.Warn("The notification channel could not be connected because the session is no longer valid.", ex);
            IsOffline = false;
        }
        catch (ServerUnreachableException ex)
        {
            ReportConnectFailure(ex);
        }
        catch (ConnectionLostException ex)
        {
            ReportConnectFailure(ex);
        }
        catch (ServerErrorException ex)
        {
            ReportConnectFailure(ex);
        }
    }

    public async Task ReturnToOngoingMatch()
    {
        FoundOngoingMatch foundMatch = await LookForOngoingMatch();
        if (foundMatch.Match is not RejoinedMatch ongoingMatch)
        {
            return;
        }

        string rivals = ListRivals(ongoingMatch.FoundMatch);
        ReturningDetailText = string.Format(CultureInfo.CurrentCulture, Strings.ReturningToMatchDetailFormat, rivals);
        IsReturningToMatch = true;
        try
        {
            await RejoinUntilBack(ongoingMatch.FoundMatch.MatchId);
        }
        finally
        {
            IsReturningToMatch = false;
        }
    }

    public void Dispose()
    {
        _isClosed = true;
        Interlocked.Exchange(ref _retryTimer, value: null)?.Dispose();
        _notifications.Disconnected -= HandleDisconnected;
    }

    private void HandleDisconnected(object? sender, EventArgs e)
    {
        UiThread.Run(_uiContext, StartReconnecting);
    }

    private void StartReconnecting()
    {
        if (_isClosed || IsOffline)
        {
            return;
        }

        _logger.Warn("The notification channel dropped in the menu; it is connected again on its own.");
        _attemptNumber = 0;
        IsOffline = true;
        ScheduleAttempt();
    }

    private void ScheduleAttempt()
    {
        if (_isClosed)
        {
            return;
        }

        Interlocked.Exchange(ref _retryTimer, value: null)?.Dispose();
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
        _ = ConnectNotifications();
    }

    private void ReportConnectFailure(Exception cause)
    {
        _logger.Warn(
            $"The notification channel could not be connected; it is tried again. Attempt={_attemptNumber}",
            cause);
        IsOffline = true;
        ScheduleAttempt();
    }

    private async Task<FoundOngoingMatch> LookForOngoingMatch()
    {
        try
        {
            return await Task.Run(FindOngoingMatch);
        }
        catch (InvalidSessionException ex)
        {
            ReportLookupFailure(ex);
        }
        catch (ServerUnreachableException ex)
        {
            ReportLookupFailure(ex);
        }
        catch (ConnectionLostException ex)
        {
            ReportLookupFailure(ex);
        }
        catch (ServerErrorException ex)
        {
            ReportLookupFailure(ex);
        }

        return FoundOngoingMatch.None;
    }

    private FoundOngoingMatch FindOngoingMatch()
    {
        return _matches.FindOngoingMatch(_session.Token);
    }

    private static void ReportLookupFailure(Exception cause)
    {
        _logger.Warn("The match in progress could not be looked up.", cause);
    }

    private string ListRivals(MatchFoundEventArgs foundMatch)
    {
        IEnumerable<string> rivalNicknames = foundMatch.Participants
            .Where(participant => !string.Equals(participant.Nickname, _session.Nickname, StringComparison.Ordinal))
            .Select(participant => participant.Nickname);
        return string.Join(RivalSeparator, rivalNicknames);
    }

    private async Task RejoinUntilBack(int matchId)
    {
        for (int attempt = 0; ; attempt++)
        {
            bool hasReturned = await TryRejoin(matchId);
            if (hasReturned)
            {
                return;
            }

            int delayIndex = Math.Min(attempt, _retryDelaysInSeconds.Length - 1);
            var delay = TimeSpan.FromSeconds(_retryDelaysInSeconds[delayIndex]);
            await Task.Delay(delay, _timeProvider);
        }
    }

    private async Task<bool> TryRejoin(int matchId)
    {
        try
        {
            RejoinedMatch rejoinedMatch = await Task.Run(() => _matches.RejoinMatch(_session.Token, matchId));
            MatchResumed?.Invoke(this, new MatchResumedEventArgs(rejoinedMatch));
            return true;
        }
        catch (MatchNotJoinedException ex)
        {
            _logger.Warn("The match in progress was over before the player came back.", ex);
            return true;
        }
        catch (InvalidSessionException ex)
        {
            ReportRejoinFailure(ex);
        }
        catch (ServerUnreachableException ex)
        {
            ReportRejoinFailure(ex);
        }
        catch (ConnectionLostException ex)
        {
            ReportRejoinFailure(ex);
        }
        catch (ServerErrorException ex)
        {
            ReportRejoinFailure(ex);
        }

        return false;
    }

    private static void ReportRejoinFailure(Exception cause)
    {
        _logger.Warn("The player could not go back to the match in progress; it is tried again.", cause);
    }
}
