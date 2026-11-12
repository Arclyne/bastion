using System;
using System.Globalization;
using System.Threading.Tasks;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Matchmaking;
using Bastion.Client.Networking.Notifications;
using Bastion.Client.Screens.MainMenu;
using Bastion.Client.Tests.Fakes;
using Xunit;

namespace Bastion.Client.Tests.Screens.MainMenu;

public sealed class TestMatchSearchViewModel : IDisposable
{
    private const string SessionToken = "session-token";
    private const string OwnNickname = "marta_muros";
    private const string RivalNickname = "leo_peon";
    private const short OwnEloRating = 1210;
    private const short RivalEloRating = 1000;
    private const byte OwnTurnOrder = 1;
    private const byte RivalTurnOrder = 2;
    private const string PreferredLanguage = "es-MX";
    private const string RejectionMessage = "The server has no callback channel for this session.";
    private const string UnfinishedMatchMessage = "The player already takes part in an unfinished match.";
    private const string ExpectedElapsedText = "00:18";
    private const int ElapsedSeconds = 18;
    private const int PresenceWindowSeconds = 60;
    private const int CountdownElapsedSeconds = 5;
    private const int ExpectedSecondsLeft = 55;
    private const int OneMatchFound = 1;
    private const int OneConfirmation = 1;
    private const int OneCancellation = 1;
    private const int NoActiveTimers = 0;

    private static readonly DateTimeOffset _searchStartedAt = new DateTimeOffset(2026, 11, 5, 18, 0, 0, TimeSpan.Zero);
    private static readonly FoundMatchParticipant _own = new FoundMatchParticipant(OwnNickname, OwnEloRating, OwnTurnOrder);
    private static readonly FoundMatchParticipant _rival =
        new FoundMatchParticipant(RivalNickname, RivalEloRating, RivalTurnOrder);
    private static readonly FoundMatchParticipant[] _foundMatchParticipants = { _own, _rival };
    private static readonly TimeSpan _presenceWindow = TimeSpan.FromSeconds(PresenceWindowSeconds);

    private readonly FakeMatchmakingClient _matchmakingClient = new FakeMatchmakingClient();
    private readonly ManualTimeProvider _timeProvider = new ManualTimeProvider(_searchStartedAt);
    private readonly MatchSearchViewModel _viewModel;
    private int _matchFoundCount;

    public TestMatchSearchViewModel()
    {
        var session = new LoggedInSession(SessionToken, OwnNickname, PreferredLanguage);
        _viewModel = InlineUiContext.CreateViewModel(
            () => new MatchSearchViewModel(session, _matchmakingClient, _timeProvider));
    }

    public void Dispose()
    {
        _viewModel.Dispose();
    }

    [Fact]
    public async Task StartSearch_Accepted_ShowsSearching()
    {
        await _viewModel.StartSearch();

        Assert.Equal(MatchSearchStage.Searching, _viewModel.Stage);
    }

    [Fact]
    public async Task StartSearch_NotificationsUnavailable_ShowsNotificationsStatus()
    {
        var cause = new InvalidOperationException(RejectionMessage);
        _matchmakingClient.StartSearchFailure = new SearchRejectedException(SearchRejection.NotificationsUnavailable, cause);

        await _viewModel.StartSearch();

        Assert.Equal(nameof(Strings.MatchSearchNotificationsUnavailable), _viewModel.StatusKey);
    }

    [Fact]
    public async Task StartSearch_UnfinishedMatch_ShowsUnfinishedMatchStatus()
    {
        var cause = new InvalidOperationException(UnfinishedMatchMessage);
        _matchmakingClient.StartSearchFailure = new SearchRejectedException(SearchRejection.UnfinishedMatch, cause);

        await _viewModel.StartSearch();

        Assert.Equal(nameof(Strings.MatchSearchUnfinishedMatch), _viewModel.StatusKey);
    }

    [Fact]
    public async Task StartSearch_ServerUnreachable_ShowsUnavailableStatus()
    {
        _matchmakingClient.StartSearchFailure = new ServerUnreachableException();

        await _viewModel.StartSearch();

        Assert.Equal(nameof(Strings.MatchSearchUnavailable), _viewModel.StatusKey);
    }

    [Fact]
    public async Task StartSearch_MatchFoundBeforeReply_KeepsMatchFound()
    {
        _matchmakingClient.MatchFoundDuringStart = _foundMatchParticipants;

        await _viewModel.StartSearch();

        Assert.Equal(MatchSearchStage.MatchFound, _viewModel.Stage);
    }

    [Fact]
    public async Task MatchFound_WhileSearching_RaisesMatchFound()
    {
        _viewModel.MatchFound += HandleMatchFound;
        await _viewModel.StartSearch();

        _matchmakingClient.RaiseMatchFound(_foundMatchParticipants);

        Assert.Equal(OneMatchFound, _matchFoundCount);
    }

    [Fact]
    public async Task MatchFound_Twice_RaisesMatchFoundOnce()
    {
        _viewModel.MatchFound += HandleMatchFound;
        await _viewModel.StartSearch();
        _matchmakingClient.RaiseMatchFound(_foundMatchParticipants);

        _matchmakingClient.RaiseMatchFound(_foundMatchParticipants);

        Assert.Equal(OneMatchFound, _matchFoundCount);
    }

    [Fact]
    public async Task MatchFound_WhileSearching_StopsSearching()
    {
        await _viewModel.StartSearch();

        _matchmakingClient.RaiseMatchFound(_foundMatchParticipants);

        Assert.False(_viewModel.IsSearching);
    }

    [Fact]
    public async Task PresenceRequested_WhileSearching_OpensDialog()
    {
        await _viewModel.StartSearch();

        _matchmakingClient.RaisePresenceRequested(_presenceWindow);

        Assert.True(_viewModel.IsPresenceDialogOpen);
    }

    [Fact]
    public async Task ClockTick_PresenceDialogOpen_CountsDownSecondsLeft()
    {
        string expectedMessage = string.Format(
            CultureInfo.CurrentCulture,
            Strings.MatchSearchPresenceMessageFormat,
            ExpectedSecondsLeft);
        await _viewModel.StartSearch();
        _matchmakingClient.RaisePresenceRequested(_presenceWindow);
        var countdownElapsed = TimeSpan.FromSeconds(CountdownElapsedSeconds);

        _timeProvider.Advance(countdownElapsed);

        Assert.Equal(expectedMessage, _viewModel.PresenceMessage);
    }

    [Fact]
    public async Task ConfirmPresence_DialogOpen_SendsConfirmation()
    {
        await _viewModel.StartSearch();
        _matchmakingClient.RaisePresenceRequested(_presenceWindow);

        await _viewModel.ConfirmPresence();

        Assert.Equal(OneConfirmation, _matchmakingClient.ConfirmPresenceCount);
    }

    [Fact]
    public async Task ConfirmPresence_DialogOpen_ClosesDialog()
    {
        await _viewModel.StartSearch();
        _matchmakingClient.RaisePresenceRequested(_presenceWindow);

        await _viewModel.ConfirmPresence();

        Assert.False(_viewModel.IsPresenceDialogOpen);
    }

    [Fact]
    public async Task SearchEnded_PresenceExpired_ShowsExpiredStatus()
    {
        await _viewModel.StartSearch();

        _matchmakingClient.RaiseSearchEnded(SearchEndReason.PresenceExpired);

        Assert.Equal(nameof(Strings.MatchSearchPresenceExpired), _viewModel.StatusKey);
    }

    [Fact]
    public async Task SearchEnded_MatchCreationFailed_ShowsCreationFailedStatus()
    {
        await _viewModel.StartSearch();

        _matchmakingClient.RaiseSearchEnded(SearchEndReason.MatchCreationFailed);

        Assert.Equal(nameof(Strings.MatchSearchMatchCreationFailed), _viewModel.StatusKey);
    }

    [Fact]
    public async Task Disconnected_WhileSearching_ShowsConnectionLostStatus()
    {
        await _viewModel.StartSearch();

        _matchmakingClient.RaiseDisconnected();

        Assert.Equal(nameof(Strings.MatchSearchConnectionLost), _viewModel.StatusKey);
    }

    [Fact]
    public async Task Disconnected_AfterMatchFound_KeepsMatchFound()
    {
        await _viewModel.StartSearch();
        _matchmakingClient.RaiseMatchFound(_foundMatchParticipants);

        _matchmakingClient.RaiseDisconnected();

        Assert.Equal(MatchSearchStage.MatchFound, _viewModel.Stage);
    }

    [Fact]
    public async Task CancelSearch_WhileSearching_StopsSearching()
    {
        await _viewModel.StartSearch();

        await _viewModel.CancelSearch();

        Assert.False(_viewModel.IsSearching);
    }

    [Fact]
    public async Task CancelSearch_WhileSearching_SendsCancellation()
    {
        await _viewModel.StartSearch();

        await _viewModel.CancelSearch();

        Assert.Equal(OneCancellation, _matchmakingClient.CancelSearchCount);
    }

    [Fact]
    public async Task ClockTick_WhileSearching_ShowsElapsedTime()
    {
        await _viewModel.StartSearch();
        var elapsed = TimeSpan.FromSeconds(ElapsedSeconds);

        _timeProvider.Advance(elapsed);

        Assert.EndsWith(ExpectedElapsedText, _viewModel.SearchDetail, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Dispose_WhileSearching_StopsClock()
    {
        await _viewModel.StartSearch();

        _viewModel.Dispose();

        int activeTimers = _timeProvider.CountActiveTimers();
        Assert.Equal(NoActiveTimers, activeTimers);
    }

    [Fact]
    public void Dispose_Subscribed_RemovesNotificationHandlers()
    {
        _viewModel.Dispose();

        bool hasSubscribers = _matchmakingClient.HasSubscribers();
        Assert.False(hasSubscribers);
    }

    private void HandleMatchFound(object? sender, MatchFoundEventArgs e)
    {
        _matchFoundCount++;
    }
}
