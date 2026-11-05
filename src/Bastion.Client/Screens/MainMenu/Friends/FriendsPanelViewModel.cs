using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Networking.Friends;
using Bastion.Client.Networking.Notifications;
using Bastion.Client.Networking.Sessions;
using Bastion.Domain.Friends;
using Gum.Mvvm;
using log4net;

namespace Bastion.Client.Screens.MainMenu.Friends;

public sealed class FriendsPanelViewModel : ViewModel
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(FriendsPanelViewModel));

    private readonly SignedInSession _session;
    private readonly IFriendClient _friends;
    private readonly SynchronizationContext? _uiContext;
    private readonly List<FriendRowViewModel> _allRows = new List<FriendRowViewModel>();

    public FriendsPanelViewModel(SignedInSession session, IFriendClient friends, INotificationClient notifications)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(friends);
        ArgumentNullException.ThrowIfNull(notifications);

        _session = session;
        _friends = friends;
        _uiContext = SynchronizationContext.Current;
        notifications.FriendPresenceChanged += HandleFriendPresenceChanged;
        SelectedTab = FriendsTab.Friends;
        FilterText = string.Empty;
        HeaderText = string.Empty;
        MenuGroupTitle = string.Empty;
        OfflineGroupTitle = string.Empty;
        OwnFriendCodeText = string.Empty;
        FormattedFriendCode = string.Empty;
        NoMatchText = string.Empty;
    }

    public bool IsOpen
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public FriendsTab SelectedTab
    {
        get => Get<FriendsTab>();
        private set => Set(value);
    }

    [DependsOn(nameof(SelectedTab))]
    public bool IsFriendsTab => SelectedTab == FriendsTab.Friends;

    [DependsOn(nameof(SelectedTab))]
    public bool IsRequestsTab => SelectedTab == FriendsTab.Requests;

    [DependsOn(nameof(SelectedTab))]
    public bool IsChatTab => SelectedTab == FriendsTab.Chat;

    [DependsOn(nameof(SelectedTab))]
    public bool IsPlaceholderTab => SelectedTab != FriendsTab.Friends;

    public string FilterText
    {
        get => Get<string>();
        set
        {
            if (Set(value))
            {
                ApplyFilter();
            }
        }
    }

    public string HeaderText
    {
        get => Get<string>();
        private set => Set(value);
    }

    public string MenuGroupTitle
    {
        get => Get<string>();
        private set => Set(value);
    }

    public string OfflineGroupTitle
    {
        get => Get<string>();
        private set => Set(value);
    }

    public string OwnFriendCodeText
    {
        get => Get<string>();
        private set => Set(value);
    }

    public string FormattedFriendCode
    {
        get => Get<string>();
        private set => Set(value);
    }

    public bool HasMenuFriends
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public bool HasOfflineFriends
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public string NoMatchText
    {
        get => Get<string>();
        private set => Set(value);
    }

    [DependsOn(nameof(NoMatchText))]
    public bool HasNoMatch => NoMatchText.Length > 0;

    public bool HasLoadFailed
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public bool IsEmpty
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public bool IsCodeCopied
    {
        get => Get<bool>();
        private set => Set(value);
    }

    [DependsOn(nameof(IsCodeCopied))]
    public string CopyButtonKey => IsCodeCopied ? nameof(Strings.FriendsCodeCopied) : nameof(Strings.FriendsCopyCode);

    public ObservableCollection<FriendRowViewModel> MenuFriends { get; } =
        new ObservableCollection<FriendRowViewModel>();

    public ObservableCollection<FriendRowViewModel> OfflineFriends { get; } =
        new ObservableCollection<FriendRowViewModel>();

    public void Open(FriendsTab tab)
    {
        SelectedTab = tab;
        IsOpen = true;
    }

    public void SelectTab(FriendsTab tab)
    {
        SelectedTab = tab;
    }

    public void Close()
    {
        IsOpen = false;
        IsCodeCopied = false;
    }

    public void MarkCodeCopied()
    {
        IsCodeCopied = true;
    }

    public async Task LoadFriends()
    {
        HasLoadFailed = false;
        try
        {
            FriendListing listing = await Task.Run(() => _friends.ListFriends(_session.Token));
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

    private static string FormatText(string format, params object[] arguments)
    {
        return string.Format(CultureInfo.CurrentCulture, format, arguments);
    }

    private void ShowListing(FriendListing listing)
    {
        _allRows.Clear();
        foreach (ListedFriend friend in listing.Friends)
        {
            _allRows.Add(new FriendRowViewModel(friend, listing.ListedAt));
        }

        FormattedFriendCode = FriendCodeRules.FormatFriendCode(listing.OwnFriendCode);
        OwnFriendCodeText = FormatText(Strings.FriendsOwnCodeFormat, FormattedFriendCode);
        IsEmpty = _allRows.Count == 0;
        UpdateHeaderText();
        ApplyFilter();
    }

    private void UpdateHeaderText()
    {
        int connectedCount = _allRows.Count(row => row.IsConnected);
        HeaderText = FormatText(Strings.FriendsConnectedCountFormat, connectedCount, _allRows.Count);
    }

    private void HandleFriendPresenceChanged(object? sender, FriendPresenceChangedEventArgs e)
    {
        if (_uiContext is null)
        {
            UpdateFriendPresence(e);
            return;
        }

        _uiContext.Post(state => UpdateFriendPresence((FriendPresenceChangedEventArgs)state!), e);
    }

    private void UpdateFriendPresence(FriendPresenceChangedEventArgs change)
    {
        int index = _allRows.FindIndex(row => row.FriendCode == change.Friend.Friend.FriendCode);
        if (index < 0)
        {
            return;
        }

        _allRows[index] = new FriendRowViewModel(change.Friend, change.OccurredAt);
        UpdateHeaderText();
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        MenuFriends.Clear();
        OfflineFriends.Clear();
        foreach (FriendRowViewModel row in _allRows.Where(MatchesFilter))
        {
            ObservableCollection<FriendRowViewModel> group = row.IsConnected ? MenuFriends : OfflineFriends;
            group.Add(row);
        }

        MenuGroupTitle = FormatText(Strings.FriendsGroupInMenuFormat, MenuFriends.Count);
        OfflineGroupTitle = FormatText(Strings.FriendsGroupOfflineFormat, OfflineFriends.Count);
        HasMenuFriends = MenuFriends.Count > 0;
        HasOfflineFriends = OfflineFriends.Count > 0;
        bool hasNoMatch = _allRows.Count > 0 && MenuFriends.Count + OfflineFriends.Count == 0;
        NoMatchText = hasNoMatch ? FormatText(Strings.FriendsNoMatchFormat, FilterText.Trim()) : string.Empty;
    }

    private bool MatchesFilter(FriendRowViewModel row)
    {
        string filter = FilterText.Trim();
        return filter.Length == 0 || row.Nickname.Contains(filter, StringComparison.OrdinalIgnoreCase);
    }

    private void ReportLoadFailure(Exception ex)
    {
        _logger.Warn("The friend list could not be loaded.", ex);
        HasLoadFailed = true;
    }
}
