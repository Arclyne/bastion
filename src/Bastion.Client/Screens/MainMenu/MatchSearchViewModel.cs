using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Accounts;
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
    private const string NicknameSeparator = ", ";

    private static readonly ILog _logger = LogManager.GetLogger(typeof(MatchSearchViewModel));
    private static readonly TimeSpan _clockTick = TimeSpan.FromSeconds(ClockTickSeconds);

    private readonly SignedInSession _session;
    private readonly IMatchmakingClient _matchmakingClient;
    private readonly TimeProvider _timeProvider;
    private readonly SynchronizationContext? _uiContext;
    private ITimer? _clockTimer;
    private DateTimeOffset _searchStartedAt;
    private DateTimeOffset _presenceDeadline;

    public MatchSearchViewModel(
        SignedInSession session,
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
        RivalText = string.Empty;
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

    [DependsOn(nameof(Stage))]
    public bool IsMatchFound => Stage == MatchSearchStage.MatchFound;

    [DependsOn(nameof(Stage))]
    public bool IsPanelVisible => Stage != MatchSearchStage.Idle;

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

    public string RivalText
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
            string? failureKey = await TrySendRequest(_matchmakingClient.StartSearch, nameof(StartSearch));
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
            string? failureKey = await TrySendRequest(_matchmakingClient.CancelSearch, nameof(CancelSearch));
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
        string? failureKey = await TrySendRequest(_matchmakingClient.ConfirmPresence, nameof(ConfirmPresence));
        if (failureKey is not null)
        {
            EndActiveSearch(failureKey);
        }
    }

    public void DismissMatchFound()
    {
        if (Stage != MatchSearchStage.MatchFound)
        {
            return;
        }

        Stage = MatchSearchStage.Idle;
    }

    public void Dispose()
    {
        StopClock();
        _matchmakingClient.MatchFound -= HandleMatchFound;
        _matchmakingClient.PresenceRequested -= HandlePresenceRequested;
        _matchmakingClient.SearchEnded -= HandleSearchEnded;
        _matchmakingClient.Disconnected -= HandleDisconnected;
    }

    private static string ToStatusKey(SearchRejection rejection)
    {
        switch (rejection)
        {
            case SearchRejection.NotificationsUnavailable:
                return nameof(Strings.MatchSearchNotificationsUnavailable);
            case SearchRejection.UnfinishedMatch:
                return nameof(Strings.MatchSearchUnfinishedMatch);
            default:
                throw new ArgumentOutOfRangeException(nameof(rejection), rejection, "Unsupported search rejection.");
        }
    }

    private static string FormatElapsed(TimeSpan elapsed)
    {
        return string.Format(CultureInfo.InvariantCulture, ElapsedFormat, (int)elapsed.TotalMinutes, elapsed.Seconds);
    }

    private async Task<string?> TrySendRequest(Action<string> request, string operationName)
    {
        try
        {
            await Task.Run(() => request(_session.Token));
            return null;
        }
        catch (InvalidSessionException ex)
        {
            return ReportRequestFailure(ex, operationName, nameof(Strings.MatchSearchSessionExpired));
        }
        catch (ServerUnreachableException ex)
        {
            return ReportRequestFailure(ex, operationName, nameof(Strings.MatchSearchUnavailable));
        }
        catch (ConnectionLostException ex)
        {
            return ReportRequestFailure(ex, operationName, nameof(Strings.MatchSearchConnectionLost));
        }
        catch (ServerErrorException ex)
        {
            return ReportRequestFailure(ex, operationName, nameof(Strings.MatchSearchUnavailable));
        }
    }

    private string ReportRequestFailure(Exception ex, string operationName, string statusKey)
    {
        _logger.Warn($"A match search request failed. Operation={operationName}", ex);
        return statusKey;
    }

    private void ShowSearching()
    {
        // The match found notice can arrive before the reply to the start request; it must not be replaced.
        if (Stage != MatchSearchStage.Idle)
        {
            return;
        }

        _searchStartedAt = _timeProvider.GetUtcNow();
        Stage = MatchSearchStage.Searching;
        UpdateClock();
        _clockTimer = _timeProvider.CreateTimer(HandleClockTick, null, _clockTick, _clockTick);
    }

    private void ShowMatchFound(IReadOnlyList<FoundMatchParticipant> participants)
    {
        if (Stage == MatchSearchStage.MatchFound)
        {
            return;
        }

        StopClock();
        IsPresenceDialogOpen = false;
        IEnumerable<string> rivalNicknames = participants.Where(IsRival).Select(participant => participant.Nickname);
        string rivals = string.Join(NicknameSeparator, rivalNicknames);
        RivalText = string.Format(CultureInfo.CurrentCulture, Strings.MatchSearchFoundDetailFormat, rivals);
        Stage = MatchSearchStage.MatchFound;
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

    private void EndActiveSearch(string statusKey)
    {
        // A match found while a request was on its way wins: only a search that is still running ends here.
        if (Stage != MatchSearchStage.Searching)
        {
            return;
        }

        StopClock();
        IsPresenceDialogOpen = false;
        Stage = MatchSearchStage.Idle;
        StatusKey = statusKey;
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

    private void UpdatePresenceMessage()
    {
        TimeSpan remaining = _presenceDeadline - _timeProvider.GetUtcNow();
        int secondsLeft = Math.Max(0, (int)Math.Ceiling(remaining.TotalSeconds));
        PresenceMessage = string.Format(
            CultureInfo.CurrentCulture,
            Strings.MatchSearchPresenceMessageFormat,
            secondsLeft);
    }

    private void StopClock()
    {
        Interlocked.Exchange(ref _clockTimer, null)?.Dispose();
    }

    private bool IsRival(FoundMatchParticipant participant)
    {
        return !string.Equals(participant.Nickname, _session.Nickname, StringComparison.Ordinal);
    }

    private void HandleClockTick(object? state)
    {
        UiThread.Run(_uiContext, UpdateClock);
    }

    private void HandleMatchFound(object? sender, MatchFoundEventArgs e)
    {
        UiThread.Run(_uiContext, () => ShowMatchFound(e.Participants));
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
}
