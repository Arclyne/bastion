using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Friends;
using Bastion.Client.Networking.Sessions;
using Gum.Mvvm;
using log4net;

namespace Bastion.Client.Screens.MainMenu.Friends;

public sealed class FriendRequestsViewModel : ViewModel
{
    private const int ToastSeconds = 8;
    private const int AcceptedToastSeconds = 3;
    private const int NewestPosition = 0;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(FriendRequestsViewModel));

    private readonly LoggedInSession _session;
    private readonly IFriendClient _friends;
    private readonly TimeProvider _timeProvider;
    private readonly List<FriendRequestRowViewModel> _receivedRequests = new List<FriendRequestRowViewModel>();
    private readonly List<FriendRequestRowViewModel> _sentRequests = new List<FriendRequestRowViewModel>();
    private FriendRequestRowViewModel? _toastRow;

    public FriendRequestsViewModel(LoggedInSession session, IFriendClient friends, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(friends);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _session = session;
        _friends = friends;
        _timeProvider = timeProvider;
        ShowsReceived = true;
        VisibleRows = new ObservableCollection<FriendRequestRowViewModel>();
        ReceivedTitle = string.Empty;
        SentTitle = string.Empty;
        EmptyText = string.Empty;
        StatusText = string.Empty;
        ToastText = string.Empty;
        PendingCountText = string.Empty;
        UpdateSummary();
    }

    public ObservableCollection<FriendRequestRowViewModel> VisibleRows { get; }

    public bool ShowsReceived
    {
        get => Get<bool>();
        private set => Set(value);
    }

    [DependsOn(nameof(ShowsReceived))]
    public bool ShowsSent => !ShowsReceived;

    public string ReceivedTitle
    {
        get => Get<string>();
        private set => Set(value);
    }

    public string SentTitle
    {
        get => Get<string>();
        private set => Set(value);
    }

    public string EmptyText
    {
        get => Get<string>();
        private set => Set(value);
    }

    [DependsOn(nameof(EmptyText))]
    public bool HasEmptyText => EmptyText.Length > 0;

    public string StatusText
    {
        get => Get<string>();
        private set => Set(value);
    }

    [DependsOn(nameof(StatusText))]
    public bool HasStatus => StatusText.Length > 0;

    public string PendingCountText
    {
        get => Get<string>();
        private set => Set(value);
    }

    [DependsOn(nameof(PendingCountText))]
    public bool HasPending => PendingCountText.Length > 0;

    public string ToastText
    {
        get => Get<string>();
        private set => Set(value);
    }

    public bool IsToastVisible
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public bool IsToastAccepted
    {
        get => Get<bool>();
        private set => Set(value);
    }

    [DependsOn(nameof(IsToastAccepted))]
    public bool AreToastActionsVisible => !IsToastAccepted;

    public event EventHandler? FriendshipStarted;

    public void ShowReceived()
    {
        ShowsReceived = true;
        UpdateSummary();
    }

    public void ShowSent()
    {
        ShowsReceived = false;
        UpdateSummary();
    }

    public async Task LoadRequests()
    {
        try
        {
            FriendRequestListing listing = await Task.Run(() => _friends.ListFriendRequests(_session.Token));
            ShowListing(listing);
        }
        catch (InvalidSessionException ex)
        {
            _logger.Warn("The friend requests could not be loaded because the session is no longer valid.", ex);
            StatusText = Strings.FriendsRequestsFailed;
        }
        catch (ServerUnreachableException ex)
        {
            ReportLoadFailure(ex);
        }
        catch (ConnectionLostException ex)
        {
            ReportLoadFailure(ex);
        }
        catch (ServerErrorException ex)
        {
            ReportLoadFailure(ex);
        }
    }

    public void ReceiveRequest(PendingFriendRequest request, bool showsToast)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (_receivedRequests.Any(row => row.FriendCode == request.Player.FriendCode))
        {
            return;
        }

        var row = new FriendRequestRowViewModel(request, request.RequestedAt, isReceived: true);
        _receivedRequests.Insert(NewestPosition, row);
        UpdateSummary();
        if (showsToast)
        {
            ShowToast(row);
        }
    }

    public Task Accept(FriendRequestRowViewModel row)
    {
        ArgumentNullException.ThrowIfNull(row);

        return Respond(row, _friends.AcceptFriendRequest, startsFriendship: true);
    }

    public Task Reject(FriendRequestRowViewModel row)
    {
        ArgumentNullException.ThrowIfNull(row);

        return Respond(row, _friends.RejectFriendRequest, startsFriendship: false);
    }

    public Task Cancel(FriendRequestRowViewModel row)
    {
        ArgumentNullException.ThrowIfNull(row);

        return Respond(row, _friends.CancelFriendRequest, startsFriendship: false);
    }

    public async Task AcceptToast()
    {
        FriendRequestRowViewModel? row = _toastRow;
        if (row is null)
        {
            return;
        }

        bool isAccepted = await Respond(row, _friends.AcceptFriendRequest, startsFriendship: true);
        if (!isAccepted)
        {
            HideToast(row);
            return;
        }

        ToastText = Strings.FriendsNowFriends;
        IsToastAccepted = true;
        await HideToastLater(row, AcceptedToastSeconds);
    }

    public async Task RejectToast()
    {
        FriendRequestRowViewModel? row = _toastRow;
        if (row is null)
        {
            return;
        }

        HideToast(row);
        await Respond(row, _friends.RejectFriendRequest, startsFriendship: false);
    }

    private void UpdateSummary()
    {
        ReceivedTitle = FormatText(Strings.FriendsReceivedFormat, _receivedRequests.Count);
        SentTitle = FormatText(Strings.FriendsSentFormat, _sentRequests.Count);
        PendingCountText = BadgeText.FormatCount(_receivedRequests.Count);

        List<FriendRequestRowViewModel> rows = ShowsReceived ? _receivedRequests : _sentRequests;
        VisibleRows.Clear();
        foreach (FriendRequestRowViewModel row in rows)
        {
            VisibleRows.Add(row);
        }

        string emptyText = ShowsReceived ? Strings.FriendsNoReceivedRequests : Strings.FriendsNoSentRequests;
        EmptyText = rows.Count == 0 ? emptyText : string.Empty;
    }

    private static string FormatText(string format, object argument)
    {
        return string.Format(CultureInfo.CurrentCulture, format, argument);
    }

    private void ShowListing(FriendRequestListing listing)
    {
        _receivedRequests.Clear();
        IEnumerable<FriendRequestRowViewModel> receivedRows = listing.ReceivedRequests.Select(
            request => new FriendRequestRowViewModel(request, listing.ListedAt, isReceived: true));
        _receivedRequests.AddRange(receivedRows);
        _sentRequests.Clear();
        IEnumerable<FriendRequestRowViewModel> sentRows = listing.SentRequests.Select(
            request => new FriendRequestRowViewModel(request, listing.ListedAt, isReceived: false));
        _sentRequests.AddRange(sentRows);
        StatusText = string.Empty;
        UpdateSummary();
    }

    private void ReportLoadFailure(Exception ex)
    {
        _logger.Error("The friend requests could not be loaded.", ex);
        StatusText = Strings.FriendsRequestsFailed;
    }

    private void ShowToast(FriendRequestRowViewModel row)
    {
        _toastRow = row;
        ToastText = FormatText(Strings.FriendsRequestToastFormat, row.Nickname);
        IsToastAccepted = false;
        IsToastVisible = true;
        _ = HideToastLater(row, ToastSeconds);
    }

    private async Task HideToastLater(FriendRequestRowViewModel row, int seconds)
    {
        var delay = TimeSpan.FromSeconds(seconds);
        await Task.Delay(delay, _timeProvider);
        HideToast(row);
    }

    private void HideToast(FriendRequestRowViewModel row)
    {
        if (_toastRow != row)
        {
            return;
        }

        _toastRow = null;
        IsToastVisible = false;
    }

    private async Task<bool> Respond(
        FriendRequestRowViewModel row,
        Action<string, string> response,
        bool startsFriendship)
    {
        row.IsBusy = true;
        StatusText = string.Empty;
        try
        {
            await Task.Run(() => response(_session.Token, row.FriendCode));
            RemoveRow(row);
            if (startsFriendship)
            {
                OnFriendshipStarted();
            }

            return true;
        }
        catch (FriendshipRejectedException ex)
        {
            _logger.Warn($"The server turned down the friend request response. Rejection={ex.Rejection}", ex);
            ShowRejection(row, ex.Rejection);
        }
        catch (InvalidSessionException ex)
        {
            _logger.Warn("The friend request response was turned down because the session is no longer valid.", ex);
            StatusText = Strings.FriendsRespondFailed;
        }
        catch (ServerUnreachableException ex)
        {
            ReportResponseFailure(ex);
        }
        catch (ConnectionLostException ex)
        {
            ReportResponseFailure(ex);
        }
        catch (ServerErrorException ex)
        {
            ReportResponseFailure(ex);
        }
        finally
        {
            row.IsBusy = false;
        }

        return false;
    }

    private void RemoveRow(FriendRequestRowViewModel row)
    {
        _receivedRequests.Remove(row);
        _sentRequests.Remove(row);
        UpdateSummary();
    }

    private void OnFriendshipStarted()
    {
        FriendshipStarted?.Invoke(this, EventArgs.Empty);
    }

    private void ShowRejection(FriendRequestRowViewModel row, FriendshipRejection rejection)
    {
        if (rejection == FriendshipRejection.RequestNotFound)
        {
            RemoveRow(row);
        }

        StatusText = FriendRejectionText.Describe(rejection);
    }

    private void ReportResponseFailure(Exception ex)
    {
        _logger.Error("The friend request response could not reach the server.", ex);
        StatusText = Strings.FriendsRespondFailed;
    }
}
