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
        StatusText = string.Empty;
    }

    public event EventHandler? FriendshipStarted;

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

    public ObservableCollection<FoundPlayerRowViewModel> Results { get; } =
        new ObservableCollection<FoundPlayerRowViewModel>();

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
            ReportFailure(ex);
        }
        catch (ServerUnreachableException ex)
        {
            ReportFailure(ex);
        }
        catch (ConnectionLostException ex)
        {
            ReportFailure(ex);
        }
        catch (ServerErrorException ex)
        {
            ReportFailure(ex);
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
            ShowRejection(row, ex);
        }
        catch (InvalidSessionException ex)
        {
            ReportFailure(ex);
        }
        catch (ServerUnreachableException ex)
        {
            ReportFailure(ex);
        }
        catch (ConnectionLostException ex)
        {
            ReportFailure(ex);
        }
        catch (ServerErrorException ex)
        {
            ReportFailure(ex);
        }
        finally
        {
            row.IsSending = false;
        }
    }

    private static string ToRejectionText(FriendshipRejection rejection)
    {
        switch (rejection)
        {
            case FriendshipRejection.PlayerNotFound:
                return Strings.FriendsRejectedNotFound;
            case FriendshipRejection.RequestAlreadySent:
                return Strings.FriendsRejectedAlreadySent;
            case FriendshipRejection.FriendLimitReached:
                return Strings.FriendsRejectedOwnLimit;
            case FriendshipRejection.RecipientFriendLimitReached:
                return Strings.FriendsRejectedTheirLimit;
            default:
                return Strings.FriendsSearchFailed;
        }
    }

    private void ShowResults(IReadOnlyList<FoundPlayer> players, string searchText)
    {
        Results.Clear();
        foreach (FoundPlayer player in players)
        {
            Results.Add(new FoundPlayerRowViewModel(player));
        }

        if (Results.Count == 0)
        {
            StatusText = string.Format(CultureInfo.CurrentCulture, Strings.FriendsSearchNoResultsFormat, searchText);
        }
    }

    private void ApplyOutcome(FoundPlayerRowViewModel row, FriendRequestOutcome outcome)
    {
        row.ApplyOutcome(outcome);
        if (outcome == FriendRequestOutcome.FriendshipStarted)
        {
            FriendshipStarted?.Invoke(this, EventArgs.Empty);
        }
    }

    private void ShowRejection(FoundPlayerRowViewModel row, FriendshipRejectedException ex)
    {
        _logger.Warn("The server turned down the friend request.", ex);
        if (ex.Rejection == FriendshipRejection.RequestAlreadySent)
        {
            row.MarkRequestSent();
        }

        StatusText = ToRejectionText(ex.Rejection);
    }

    private void ReportFailure(Exception ex)
    {
        _logger.Warn("The friend search could not reach the server.", ex);
        StatusText = Strings.FriendsSearchFailed;
    }
}
