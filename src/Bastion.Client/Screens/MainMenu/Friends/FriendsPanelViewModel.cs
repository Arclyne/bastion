using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Friends;
using Bastion.Client.Networking.Notifications;
using Bastion.Client.Networking.Sessions;
using Bastion.Client.Screens.MainMenu.Chat;
using Bastion.Domain.Friends;
using Gum.Mvvm;
using log4net;

namespace Bastion.Client.Screens.MainMenu.Friends;

public sealed class FriendsPanelViewModel : ViewModel, IFriendDirectory, IDisposable
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(FriendsPanelViewModel));

    private readonly LoggedInSession _session;
    private readonly IFriendClient _friends;
    private readonly INotificationClient _notifications;
    private readonly IClipboard _clipboard;
    private readonly SynchronizationContext? _uiContext;
    private readonly List<FriendRowViewModel> _allRows = new List<FriendRowViewModel>();

    public FriendsPanelViewModel(LoggedInSession session, SocialClients clients, LocalServices localServices)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(clients);
        ArgumentNullException.ThrowIfNull(localServices);

        _session = session;
        _friends = clients.Friends;
        _notifications = clients.Notifications;
        _clipboard = localServices.Clipboard;
        _uiContext = SynchronizationContext.Current;
        AddFriend = new AddFriendViewModel(session, clients.Friends);
        AddFriend.FriendshipStarted += HandleFriendshipStarted;
        Actions = new FriendActionsViewModel(session, clients.Friends);
        Actions.FriendRemoved += HandleFriendRemoved;
        Requests = new FriendRequestsViewModel(session, clients.Friends, localServices.TimeProvider);
        Requests.FriendshipStarted += HandleFriendshipStarted;
        Actions.MessageRequested += HandleMessageRequested;
        var chatContext = new ChatContext(session, clients.Chat, localServices.TimeProvider);
        Chat = new ChatTabViewModel(chatContext, this);
        _notifications.FriendPresenceChanged += HandleFriendPresenceChanged;
        _notifications.FriendRequestReceived += HandleFriendRequestReceived;
        _notifications.FriendshipStarted += HandleRemoteFriendshipStarted;
        ConnectedFriends = new ObservableCollection<FriendRowViewModel>();
        OfflineFriends = new ObservableCollection<FriendRowViewModel>();
        SelectedTab = FriendsTab.Friends;
        FilterText = string.Empty;
        HeaderText = string.Empty;
        ConnectedGroupTitle = string.Empty;
        OfflineGroupTitle = string.Empty;
        OwnFriendCodeText = string.Empty;
        FormattedFriendCode = string.Empty;
        NoMatchText = string.Empty;
    }

    public AddFriendViewModel AddFriend { get; }

    public FriendRequestsViewModel Requests { get; }

    public FriendActionsViewModel Actions { get; }

    public ChatTabViewModel Chat { get; }

    public bool IsOpen
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public bool IsAddingFriend
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
    public bool ShowsFriendCode => SelectedTab != FriendsTab.Chat;

    [DependsOn(nameof(SelectedTab))]
    [DependsOn(nameof(IsAddingFriend))]
    public bool IsFriendListVisible => IsFriendsTab && !IsAddingFriend;

    [DependsOn(nameof(SelectedTab))]
    [DependsOn(nameof(IsAddingFriend))]
    public bool IsAddFriendVisible => IsFriendsTab && IsAddingFriend;

    public string FilterText
    {
        get => Get<string>();
        set
        {
            if (!Set(value))
            {
                return;
            }

            ApplyFilter();
        }
    }

    public string HeaderText
    {
        get => Get<string>();
        private set => Set(value);
    }

    public string ConnectedGroupTitle
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

    public bool HasConnectedFriends
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

    public ObservableCollection<FriendRowViewModel> ConnectedFriends { get; }

    public ObservableCollection<FriendRowViewModel> OfflineFriends { get; }

    public void Open(FriendsTab tab)
    {
        SelectTab(tab);
        IsOpen = true;
    }

    public void SelectTab(FriendsTab tab)
    {
        SelectedTab = tab;
        if (tab == FriendsTab.Chat)
        {
            _ = Chat.Load();
        }
    }

    public void Close()
    {
        IsOpen = false;
        IsCodeCopied = false;
        IsAddingFriend = false;
        Actions.CloseAll();
    }

    public void OpenAddFriend()
    {
        AddFriend.Reset();
        IsAddingFriend = true;
    }

    public void CloseAddFriend()
    {
        IsAddingFriend = false;
    }

    public void CopyOwnCode()
    {
        bool isCopied = _clipboard.TryCopyText(FormattedFriendCode);
        if (!isCopied)
        {
            _logger.Warn("The friend code could not be copied because there is no clipboard.");
            return;
        }

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
            _logger.Warn("The friend list could not be loaded because the session is no longer valid.", ex);
            HasLoadFailed = true;
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

    public ConversationPresence? FindPresence(string friendCode)
    {
        ArgumentNullException.ThrowIfNull(friendCode);

        FriendRowViewModel? row = _allRows.Find(friend => friend.FriendCode == friendCode);
        if (row is null)
        {
            return null;
        }

        return new ConversationPresence(row.IsConnected, row.StatusText);
    }

    public void Dispose()
    {
        _notifications.FriendPresenceChanged -= HandleFriendPresenceChanged;
        _notifications.FriendRequestReceived -= HandleFriendRequestReceived;
        _notifications.FriendshipStarted -= HandleRemoteFriendshipStarted;
        Chat.Dispose();
    }

    private void ApplyFilter()
    {
        ConnectedFriends.Clear();
        OfflineFriends.Clear();
        IEnumerable<FriendRowViewModel> matchingRows = _allRows.Where(MatchesFilter);
        foreach (FriendRowViewModel row in matchingRows)
        {
            ObservableCollection<FriendRowViewModel> group = row.IsConnected ? ConnectedFriends : OfflineFriends;
            group.Add(row);
        }

        ConnectedGroupTitle = FormatText(Strings.FriendsGroupInMenuFormat, ConnectedFriends.Count);
        OfflineGroupTitle = FormatText(Strings.FriendsGroupOfflineFormat, OfflineFriends.Count);
        HasConnectedFriends = ConnectedFriends.Count > 0;
        HasOfflineFriends = OfflineFriends.Count > 0;
        bool hasNoMatch = _allRows.Count > 0 && ConnectedFriends.Count + OfflineFriends.Count == 0;
        NoMatchText = hasNoMatch ? FormatText(Strings.FriendsNoMatchFormat, FilterText.Trim()) : string.Empty;
    }

    private bool MatchesFilter(FriendRowViewModel row)
    {
        string filter = FilterText.Trim();
        return filter.Length == 0 || row.Nickname.Contains(filter, StringComparison.OrdinalIgnoreCase);
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
            var row = new FriendRowViewModel(friend, listing.ListedAt);
            _allRows.Add(row);
        }

        FormattedFriendCode = FriendCodePolicy.Format(listing.OwnFriendCode);
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

    private void ReportLoadFailure(Exception ex)
    {
        _logger.Error("The friend list could not be loaded.", ex);
        HasLoadFailed = true;
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
        Chat.RefreshPresence(change.Friend.Friend.FriendCode);
    }

    private void ReceiveFriendRequest(PendingFriendRequest request)
    {
        bool isRequestsTabShown = IsOpen && IsRequestsTab;
        Requests.ReceiveRequest(request, !isRequestsTabShown);
    }

    private void ReloadFriendsAndRequests()
    {
        _ = LoadFriends();
        _ = Requests.LoadRequests();
    }

    private void HandleFriendshipStarted(object? sender, EventArgs e)
    {
        _ = LoadFriends();
    }

    private void HandleFriendRemoved(object? sender, FriendRemovedEventArgs e)
    {
        _allRows.RemoveAll(row => row.FriendCode == e.FriendCode);
        IsEmpty = _allRows.Count == 0;
        UpdateHeaderText();
        ApplyFilter();
    }

    private void HandleMessageRequested(object? sender, MessageRequestedEventArgs e)
    {
        Actions.CloseAll();
        SelectTab(FriendsTab.Chat);
        Chat.OpenPrivate(e.Friend);
    }

    private void HandleFriendPresenceChanged(object? sender, FriendPresenceChangedEventArgs e)
    {
        UiThread.Run(_uiContext, () => UpdateFriendPresence(e));
    }

    private void HandleFriendRequestReceived(object? sender, FriendRequestReceivedEventArgs e)
    {
        UiThread.Run(_uiContext, () => ReceiveFriendRequest(e.Request));
    }

    private void HandleRemoteFriendshipStarted(object? sender, FriendshipStartedEventArgs e)
    {
        UiThread.Run(_uiContext, ReloadFriendsAndRequests);
    }
}
