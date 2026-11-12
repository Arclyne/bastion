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

    private static readonly ILog _logger = LogManager.GetLogger(typeof(FriendRequestsViewModel));

    private readonly LoggedInSession _session;
    private readonly IFriendClient _friends;
    private readonly List<FriendRequestRowViewModel> _received = new List<FriendRequestRowViewModel>();
    private readonly List<FriendRequestRowViewModel> _sent = new List<FriendRequestRowViewModel>();
    private FriendRequestRowViewModel? _toastRow;

    public FriendRequestsViewModel(LoggedInSession session, IFriendClient friends)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(friends);

        _session = session;
        _friends = friends;
        ShowsReceived = true;
        ReceivedTitle = string.Empty;
        SentTitle = string.Empty;
        EmptyText = string.Empty;
        StatusText = string.Empty;
        ToastText = string.Empty;
        PendingCountText = string.Empty;
        UpdateSummary();
    }

    public event EventHandler? FriendshipStarted;

    public ObservableCollection<FriendRequestRowViewModel> VisibleRows { get; } =
        new ObservableCollection<FriendRequestRowViewModel>();

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
            ReportLoadFailure(ex);
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

        if (_received.Any(row => row.FriendCode == request.Player.FriendCode))
        {
            return;
        }

        var row = new FriendRequestRowViewModel(request, request.RequestedAt, true);
        _received.Insert(0, row);
        UpdateSummary();
        if (showsToast)
        {
            ShowToast(row);
        }
    }

    public Task Accept(FriendRequestRowViewModel row)
    {
        return Respond(row, (token, code) => _friends.AcceptFriendRequest(token, code), true);
    }

    public Task Reject(FriendRequestRowViewModel row)
    {
        return Respond(row, (token, code) => _friends.RejectFriendRequest(token, code), false);
    }

    public Task Cancel(FriendRequestRowViewModel row)
    {
        return Respond(row, (token, code) => _friends.CancelFriendRequest(token, code), false);
    }

    public async Task AcceptToast()
    {
        FriendRequestRowViewModel? row = _toastRow;
        if (row is null)
        {
            return;
        }

        bool isAccepted = await Respond(row, (token, code) => _friends.AcceptFriendRequest(token, code), true);
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
        await Respond(row, (token, code) => _friends.RejectFriendRequest(token, code), false);
    }

    private static string FormatText(string format, object argument)
    {
        return string.Format(CultureInfo.CurrentCulture, format, argument);
    }

    private async Task<bool> Respond(
        FriendRequestRowViewModel row,
        Action<string, string> response,
        bool startsFriendship)
    {
        ArgumentNullException.ThrowIfNull(row);

        row.IsBusy = true;
        StatusText = string.Empty;
        try
        {
            await Task.Run(() => response(_session.Token, row.FriendCode));
            RemoveRow(row);
            RaiseFriendshipStarted(startsFriendship);
            return true;
        }
        catch (FriendshipRejectedException ex)
        {
            ShowRejection(row, ex);
        }
        catch (InvalidSessionException ex)
        {
            ReportResponseFailure(ex);
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

    private void ShowListing(FriendRequestListing listing)
    {
        _received.Clear();
        _received.AddRange(listing.ReceivedRequests.Select(request =>
            new FriendRequestRowViewModel(request, listing.ListedAt, true)));
        _sent.Clear();
        _sent.AddRange(listing.SentRequests.Select(request =>
            new FriendRequestRowViewModel(request, listing.ListedAt, false)));
        StatusText = string.Empty;
        UpdateSummary();
    }

    private void UpdateSummary()
    {
        ReceivedTitle = FormatText(Strings.FriendsReceivedFormat, _received.Count);
        SentTitle = FormatText(Strings.FriendsSentFormat, _sent.Count);
        PendingCountText = _received.Count > 0 ? _received.Count.ToString(CultureInfo.CurrentCulture) : string.Empty;

        List<FriendRequestRowViewModel> rows = ShowsReceived ? _received : _sent;
        VisibleRows.Clear();
        foreach (FriendRequestRowViewModel row in rows)
        {
            VisibleRows.Add(row);
        }

        string emptyText = ShowsReceived ? Strings.FriendsNoReceivedRequests : Strings.FriendsNoSentRequests;
        EmptyText = rows.Count == 0 ? emptyText : string.Empty;
    }

    private void RemoveRow(FriendRequestRowViewModel row)
    {
        _received.Remove(row);
        _sent.Remove(row);
        UpdateSummary();
    }

    private void RaiseFriendshipStarted(bool startsFriendship)
    {
        if (startsFriendship)
        {
            FriendshipStarted?.Invoke(this, EventArgs.Empty);
        }
    }

    private void ShowRejection(FriendRequestRowViewModel row, FriendshipRejectedException ex)
    {
        _logger.Warn("The server turned down the friend request response.", ex);
        if (ex.Rejection == FriendshipRejection.RequestNotFound)
        {
            RemoveRow(row);
            StatusText = Strings.FriendsRejectedRequestNotFound;
            return;
        }

        StatusText = ex.Rejection == FriendshipRejection.FriendLimitReached
            ? Strings.FriendsRejectedOwnLimit
            : Strings.FriendsRejectedTheirLimit;
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
        await Task.Delay(TimeSpan.FromSeconds(seconds));
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

    private void ReportLoadFailure(Exception ex)
    {
        _logger.Warn("The friend requests could not be loaded.", ex);
        StatusText = Strings.FriendsRequestsFailed;
    }

    private void ReportResponseFailure(Exception ex)
    {
        _logger.Warn("The friend request response could not reach the server.", ex);
        StatusText = Strings.FriendsSearchFailed;
    }
}
