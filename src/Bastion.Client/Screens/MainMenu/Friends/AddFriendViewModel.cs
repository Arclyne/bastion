using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Friends;
using Bastion.Client.Networking.Sessions;
using Gum.Mvvm;
using log4net;

namespace Bastion.Client.Screens.MainMenu.Friends;

public sealed class AddFriendViewModel : ViewModel
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(AddFriendViewModel));

    private readonly LoggedInSession _session;
    private readonly IFriendClient _friends;

    public AddFriendViewModel(LoggedInSession session, IFriendClient friends)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(friends);

        _session = session;
        _friends = friends;
        SearchText = string.Empty;
        Results = new ObservableCollection<FoundPlayerRowViewModel>();
        StatusText = string.Empty;
    }

    public string SearchText
    {
        get => Get<string>();
        set => Set(value);
    }

    public bool IsSearching
    {
        get => Get<bool>();
        private set => Set(value);
    }

    [DependsOn(nameof(IsSearching))]
    public bool CanSearch => !IsSearching;

    public string StatusText
    {
        get => Get<string>();
        private set => Set(value);
    }

    [DependsOn(nameof(StatusText))]
    public bool HasStatus => StatusText.Length > 0;

    public ObservableCollection<FoundPlayerRowViewModel> Results { get; }

    public event EventHandler? FriendshipStarted;

    public void Reset()
    {
        SearchText = string.Empty;
        StatusText = string.Empty;
        Results.Clear();
    }

    public async Task SearchPlayers()
    {
        if (IsSearching)
        {
            return;
        }

        string searchText = SearchText.Trim();
        IsSearching = true;
        StatusText = string.Empty;
        try
        {
            IReadOnlyList<FoundPlayer> players =
                await Task.Run(() => _friends.SearchPlayers(_session.Token, searchText));
            ShowResults(players, searchText);
        }
        catch (InvalidSessionException ex)
        {
            _logger.Warn("The friend search was turned down because the session is no longer valid.", ex);
            StatusText = Strings.FriendsSearchFailed;
        }
        catch (ServerUnreachableException ex)
        {
            ReportSearchFailure(ex);
        }
        catch (ConnectionLostException ex)
        {
            ReportSearchFailure(ex);
        }
        catch (ServerErrorException ex)
        {
            ReportSearchFailure(ex);
        }
        finally
        {
            IsSearching = false;
        }
    }

    public async Task SendRequest(FoundPlayerRowViewModel row)
    {
        ArgumentNullException.ThrowIfNull(row);

        if (!row.CanPress)
        {
            return;
        }

        row.IsSending = true;
        StatusText = string.Empty;
        try
        {
            FriendRequestOutcome outcome =
                await Task.Run(() => _friends.SendFriendRequest(_session.Token, row.FriendCode));
            ApplyOutcome(row, outcome);
        }
        catch (FriendshipRejectedException ex)
        {
            _logger.Warn($"The server turned down the friend request. Rejection={ex.Rejection}", ex);
            ShowRejection(row, ex.Rejection);
        }
        catch (InvalidSessionException ex)
        {
            _logger.Warn("The friend request was turned down because the session is no longer valid.", ex);
            StatusText = Strings.FriendsSendRequestFailed;
        }
        catch (ServerUnreachableException ex)
        {
            ReportSendFailure(ex);
        }
        catch (ConnectionLostException ex)
        {
            ReportSendFailure(ex);
        }
        catch (ServerErrorException ex)
        {
            ReportSendFailure(ex);
        }
        finally
        {
            row.IsSending = false;
        }
    }

    private void ShowResults(IReadOnlyList<FoundPlayer> players, string searchText)
    {
        Results.Clear();
        foreach (FoundPlayer player in players)
        {
            var row = new FoundPlayerRowViewModel(player);
            Results.Add(row);
        }

        if (Results.Count == 0)
        {
            StatusText = string.Format(CultureInfo.CurrentCulture, Strings.FriendsSearchNoResultsFormat, searchText);
        }
    }

    private void ReportSearchFailure(Exception ex)
    {
        _logger.Error("The friend search could not reach the server.", ex);
        StatusText = Strings.FriendsSearchFailed;
    }

    private void ApplyOutcome(FoundPlayerRowViewModel row, FriendRequestOutcome outcome)
    {
        row.ApplyOutcome(outcome);
        if (outcome == FriendRequestOutcome.FriendshipStarted)
        {
            FriendshipStarted?.Invoke(this, EventArgs.Empty);
        }
    }

    private void ShowRejection(FoundPlayerRowViewModel row, FriendshipRejection rejection)
    {
        if (rejection == FriendshipRejection.RequestAlreadySent)
        {
            row.MarkRequestSent();
        }

        StatusText = FriendRejectionText.Describe(rejection);
    }

    private void ReportSendFailure(Exception ex)
    {
        _logger.Error("The friend request could not reach the server.", ex);
        StatusText = Strings.FriendsSendRequestFailed;
    }

}
