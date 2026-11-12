using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Matchmaking;
using Bastion.Client.Networking.Notifications;
using Bastion.Client.Networking.Sessions;
using Gum.Mvvm;
using log4net;

namespace Bastion.Client.Screens.MainMenu;

public sealed class MatchSearchViewModel : ViewModel, IDisposable
{
    private const int ClockTickSeconds = 1;
    private const string ElapsedFormat = "{0:00}:{1:00}";
    private const string DetailSeparator = " · ";
    private const int MinimumSecondsLeft = 0;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(MatchSearchViewModel));
    private static readonly TimeSpan _clockTick = TimeSpan.FromSeconds(ClockTickSeconds);

    private readonly LoggedInSession _session;
    private readonly IMatchmakingClient _matchmakingClient;
    private readonly TimeProvider _timeProvider;
    private readonly SynchronizationContext? _uiContext;
    private ITimer? _clockTimer;
    private DateTimeOffset _searchStartedAt;
    private DateTimeOffset _presenceDeadline;

    public MatchSearchViewModel(
        LoggedInSession session,
        IMatchmakingClient matchmakingClient,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(matchmakingClient);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _session = session;
        _matchmakingClient = matchmakingClient;
        _timeProvider = timeProvider;
        _uiContext = SynchronizationContext.Current;
        Stage = MatchSearchStage.Idle;
        SearchDetail = string.Empty;
        PresenceMessage = string.Empty;
        StatusKey = string.Empty;
        _matchmakingClient.MatchFound += HandleMatchFound;
        _matchmakingClient.PresenceRequested += HandlePresenceRequested;
        _matchmakingClient.SearchEnded += HandleSearchEnded;
        _matchmakingClient.Disconnected += HandleDisconnected;
    }

    public MatchSearchStage Stage
    {
        get => Get<MatchSearchStage>();
        private set => Set(value);
    }

    [DependsOn(nameof(Stage))]
    public bool IsSearching => Stage == MatchSearchStage.Searching;

    public bool IsBusy
    {
        get => Get<bool>();
        private set => Set(value);
    }

    [DependsOn(nameof(Stage))]
    [DependsOn(nameof(IsBusy))]
    public bool CanStartSearch => Stage == MatchSearchStage.Idle && !IsBusy;

    [DependsOn(nameof(Stage))]
    [DependsOn(nameof(IsBusy))]
    public bool CanCancelSearch => Stage == MatchSearchStage.Searching && !IsBusy;

    public string SearchDetail
    {
        get => Get<string>();
        private set => Set(value);
    }

    public bool IsPresenceDialogOpen
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public string PresenceMessage
    {
        get => Get<string>();
        private set => Set(value);
    }

    public string StatusKey
    {
        get => Get<string>();
        private set => Set(value);
    }

    [DependsOn(nameof(StatusKey))]
    public bool HasStatus => StatusKey.Length > 0;

    public event EventHandler<MatchFoundEventArgs>? MatchFound;

    public async Task StartSearch()
    {
        if (!CanStartSearch)
        {
            return;
        }

        StatusKey = string.Empty;
        IsBusy = true;
        try
        {
            string? failureKey = await SendRequest(_matchmakingClient.StartSearch, nameof(StartSearch));
            if (failureKey is not null)
            {
                StatusKey = failureKey;
                return;
            }

            ShowSearching();
        }
        catch (SearchRejectedException ex)
        {
            _logger.Warn($"The server rejected the match search. Rejection={ex.Rejection}", ex);
            StatusKey = ToStatusKey(ex.Rejection);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task CancelSearch()
    {
        if (!CanCancelSearch)
        {
            return;
        }

        IsBusy = true;
        try
        {
            string? failureKey = await SendRequest(_matchmakingClient.CancelSearch, nameof(CancelSearch));
            EndActiveSearch(failureKey ?? string.Empty);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task ConfirmPresence()
    {
        if (!IsPresenceDialogOpen)
        {
            return;
        }

        IsPresenceDialogOpen = false;
        string? failureKey = await SendRequest(_matchmakingClient.ConfirmPresence, nameof(ConfirmPresence));
        if (failureKey is not null)
        {
            EndActiveSearch(failureKey);
        }
    }

    public void Dispose()
    {
        StopClock();
        _matchmakingClient.MatchFound -= HandleMatchFound;
        _matchmakingClient.PresenceRequested -= HandlePresenceRequested;
        _matchmakingClient.SearchEnded -= HandleSearchEnded;
        _matchmakingClient.Disconnected -= HandleDisconnected;
    }

    private async Task<string?> SendRequest(Action<string> request, string operationName)
    {
        try
        {
            await Task.Run(() => request(_session.Token));
            return null;
        }
        catch (InvalidSessionException ex)
        {
            _logger.Warn($"The server turned down a match search request with an invalid session. Operation={operationName}", ex);
            return nameof(Strings.MatchSearchSessionExpired);
        }
        catch (ServerUnreachableException ex)
        {
            LogRequestFailure(ex, operationName);
            return nameof(Strings.MatchSearchUnavailable);
        }
        catch (ConnectionLostException ex)
        {
            LogRequestFailure(ex, operationName);
            return nameof(Strings.MatchSearchConnectionLost);
        }
        catch (ServerErrorException ex)
        {
            LogRequestFailure(ex, operationName);
            return nameof(Strings.MatchSearchUnavailable);
        }
    }

    private static void LogRequestFailure(Exception ex, string operationName)
    {
        _logger.Error($"A match search request failed. Operation={operationName}", ex);
    }

    private void ShowSearching()
    {
        if (Stage != MatchSearchStage.Idle)
        {
            return;
        }

        _searchStartedAt = _timeProvider.GetUtcNow();
        Stage = MatchSearchStage.Searching;
        UpdateClock();
        _clockTimer = _timeProvider.CreateTimer(HandleClockTick, null, _clockTick, _clockTick);
    }

    private void UpdateClock()
    {
        if (Stage != MatchSearchStage.Searching)
        {
            return;
        }

        TimeSpan elapsed = _timeProvider.GetUtcNow() - _searchStartedAt;
        SearchDetail = string.Concat(Strings.MainMenuFindMatchDetail, DetailSeparator, FormatElapsed(elapsed));
        if (IsPresenceDialogOpen)
        {
            UpdatePresenceMessage();
        }
    }

    private static string FormatElapsed(TimeSpan elapsed)
    {
        double totalMinutes = elapsed.TotalMinutes;
        int wholeMinutes = (int)totalMinutes;
        return string.Format(CultureInfo.InvariantCulture, ElapsedFormat, wholeMinutes, elapsed.Seconds);
    }

    private void UpdatePresenceMessage()
    {
        TimeSpan remaining = _presenceDeadline - _timeProvider.GetUtcNow();
        double roundedUpSeconds = Math.Ceiling(remaining.TotalSeconds);
        int wholeSecondsLeft = (int)roundedUpSeconds;
        int secondsLeft = Math.Max(MinimumSecondsLeft, wholeSecondsLeft);
        PresenceMessage = string.Format(
            CultureInfo.CurrentCulture,
            Strings.MatchSearchPresenceMessageFormat,
            secondsLeft);
    }

    private static string ToStatusKey(SearchRejection rejection)
    {
        switch (rejection)
        {
            case SearchRejection.NotificationsUnavailable:
                return nameof(Strings.MatchSearchNotificationsUnavailable);
            case SearchRejection.UnfinishedMatch:
                return nameof(Strings.MatchSearchUnfinishedMatch);
            case SearchRejection.InRoom:
                return nameof(Strings.MatchSearchInRoom);
            default:
                throw new ArgumentOutOfRangeException(nameof(rejection), rejection, "Unsupported search rejection.");
        }
    }

    private void EndActiveSearch(string statusKey)
    {
        if (Stage != MatchSearchStage.Searching)
        {
            return;
        }

        StopClock();
        IsPresenceDialogOpen = false;
        Stage = MatchSearchStage.Idle;
        StatusKey = statusKey;
    }

    private void StopClock()
    {
        Interlocked.Exchange(ref _clockTimer, null)?.Dispose();
    }

    private void EndSearchWithMatch(MatchFoundEventArgs foundMatch)
    {
        if (Stage == MatchSearchStage.MatchFound)
        {
            return;
        }

        StopClock();
        IsPresenceDialogOpen = false;
        Stage = MatchSearchStage.MatchFound;
        OnMatchFound(foundMatch);
    }

    private void OnMatchFound(MatchFoundEventArgs foundMatch)
    {
        MatchFound?.Invoke(this, foundMatch);
    }

    private void ShowPresenceCheck(TimeSpan responseWindow)
    {
        if (Stage != MatchSearchStage.Searching)
        {
            return;
        }

        _presenceDeadline = _timeProvider.GetUtcNow() + responseWindow;
        UpdatePresenceMessage();
        IsPresenceDialogOpen = true;
    }

    private void FinishSearch(SearchEndReason reason)
    {
        switch (reason)
        {
            case SearchEndReason.Cancelled:
                EndActiveSearch(string.Empty);
                break;
            case SearchEndReason.PresenceExpired:
                EndActiveSearch(nameof(Strings.MatchSearchPresenceExpired));
                break;
            case SearchEndReason.MatchCreationFailed:
                EndActiveSearch(nameof(Strings.MatchSearchMatchCreationFailed));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(reason), reason, "Unsupported search end reason.");
        }
    }

    private void HandleMatchFound(object? sender, MatchFoundEventArgs e)
    {
        UiThread.Run(_uiContext, () => EndSearchWithMatch(e));
    }

    private void HandlePresenceRequested(object? sender, PresenceRequestedEventArgs e)
    {
        UiThread.Run(_uiContext, () => ShowPresenceCheck(e.ResponseWindow));
    }

    private void HandleSearchEnded(object? sender, SearchEndedEventArgs e)
    {
        UiThread.Run(_uiContext, () => FinishSearch(e.Reason));
    }

    private void HandleDisconnected(object? sender, EventArgs e)
    {
        UiThread.Run(_uiContext, () => EndActiveSearch(nameof(Strings.MatchSearchConnectionLost)));
    }

    private void HandleClockTick(object? state)
    {
        UiThread.Run(_uiContext, UpdateClock);
    }
}
