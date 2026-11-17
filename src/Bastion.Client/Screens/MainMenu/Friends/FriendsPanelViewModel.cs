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
    private readonly List<FriendRowViewModel> _allRows;
    private string _formattedFriendCode;

    public FriendsPanelViewModel(LoggedInSession session, SocialClientSet clients, LocalServiceSet localServices)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(clients);
        ArgumentNullException.ThrowIfNull(localServices);

        _session = session;
        _friends = clients.Friends;
        _notifications = clients.Chat.Notifications;
        _clipboard = localServices.Clipboard;
        _uiContext = SynchronizationContext.Current;
        _allRows = new List<FriendRowViewModel>();
        _formattedFriendCode = string.Empty;
        AddFriend = new AddFriendViewModel(session, clients.Friends);
        Actions = new FriendActionsViewModel(session, clients.Friends);
        Requests = new FriendRequestsViewModel(session, clients.Friends, localServices.TimeProvider);
        ConnectedFriends = new ObservableCollection<FriendRowViewModel>();
        OfflineFriends = new ObservableCollection<FriendRowViewModel>();
        SelectedTab = FriendsTab.Friends;
        FilterText = string.Empty;
        HeaderText = string.Empty;
        ConnectedGroupTitle = string.Empty;
        OfflineGroupTitle = string.Empty;
        OwnFriendCodeText = string.Empty;
        NoMatchText = string.Empty;
        var chatContext = new ChatContext(session, clients.Chat, localServices.TimeProvider);
        Chat = new ChatTabViewModel(chatContext, this);
        AddFriend.FriendshipStarted += HandleFriendshipStarted;
        Actions.FriendRemoved += HandleFriendRemoved;
        Requests.FriendshipStarted += HandleFriendshipStarted;
        Actions.MessageRequested += HandleMessageRequested;
        _notifications.FriendPresenceChanged += HandleFriendPresenceChanged;
        _notifications.FriendRequestReceived += HandleFriendRequestReceived;
        _notifications.FriendshipStarted += HandleRemoteFriendshipStarted;
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

    private bool IsAddingFriend
    {
        get => Get<bool>();
        set => Set(value);
    }

    private FriendsTab SelectedTab
    {
        get => Get<FriendsTab>();
        set => Set(value);
    }

    [DependsOn(nameof(SelectedTab))]
    public bool IsFriendsTab => SelectedTab == FriendsTab.Friends;

    [DependsOn(nameof(SelectedTab))]
    public bool IsRequestsTab => SelectedTab == FriendsTab.Requests;

    [DependsOn(nameof(SelectedTab))]
    public bool IsChatTab => SelectedTab == FriendsTab.Chat;

    [DependsOn(nameof(SelectedTab))]
    public bool IsFriendCodeVisible => SelectedTab != FriendsTab.Chat;

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
            if (value is null || !Set(value))
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
            _ = RunSafely(Chat.Load());
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
        bool isCopied = _clipboard.TryCopyText(_formattedFriendCode);
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

    public ConversationPresence GetPresence(string friendCode)
    {
        ArgumentNullException.ThrowIfNull(friendCode);

        FriendRowViewModel? row = _allRows.Find(friend => friend.FriendCode == friendCode);
        if (row is null)
        {
            return ConversationPresence.CreateForNonFriend();
        }

        PartnerPresence presence = row.IsConnected ? PartnerPresence.Connected : PartnerPresence.Disconnected;
        return new ConversationPresence(presence, row.StatusText);
    }

    public void Dispose()
    {
        _notifications.FriendPresenceChanged -= HandleFriendPresenceChanged;
        _notifications.FriendRequestReceived -= HandleFriendRequestReceived;
        _notifications.FriendshipStarted -= HandleRemoteFriendshipStarted;
        Chat.Dispose();
    }

    private void HandleFriendshipStarted(object? sender, EventArgs e)
    {
        _ = RunSafely(LoadFriends());
    }

    private static async Task RunSafely(Task task)
    {
        try
        {
            await task;
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("A friends panel load ended because the connection was closed.", ex);
        }
    }

    private void HandleFriendRemoved(object? sender, FriendRemovedEventArgs e)
    {
        _allRows.RemoveAll(row => row.FriendCode == e.FriendCode);
        RefreshFriendGroups();
    }

    private void RefreshFriendGroups()
    {
        IsEmpty = _allRows.Count == 0;
        UpdateHeaderText();
        ApplyFilter();
    }

    private void UpdateHeaderText()
    {
        int connectedCount = _allRows.Count(row => row.IsConnected);
        HeaderText = FormatText(Strings.FriendsConnectedCountFormat, connectedCount, _allRows.Count);
    }

    private static string FormatText(string format, params object[] arguments)
    {
        return string.Format(CultureInfo.CurrentCulture, format, arguments);
    }

    private void ApplyFilter()
    {
        ConnectedFriends.Clear();
        OfflineFriends.Clear();
        IEnumerable<FriendRowViewModel> matchingRows = _allRows.Where(IsMatchingFilter);
        foreach (FriendRowViewModel row in matchingRows)
        {
            ObservableCollection<FriendRowViewModel> groupRows = row.IsConnected ? ConnectedFriends : OfflineFriends;
            groupRows.Add(row);
        }

        ConnectedGroupTitle = FormatText(Strings.FriendsGroupInMenuFormat, ConnectedFriends.Count);
        OfflineGroupTitle = FormatText(Strings.FriendsGroupOfflineFormat, OfflineFriends.Count);
        HasConnectedFriends = ConnectedFriends.Count > 0;
        HasOfflineFriends = OfflineFriends.Count > 0;
        bool hasNoMatch = _allRows.Count > 0 && ConnectedFriends.Count + OfflineFriends.Count == 0;
        string filter = FilterText.Trim();
        NoMatchText = hasNoMatch ? FormatText(Strings.FriendsNoMatchFormat, filter) : string.Empty;
    }

    private bool IsMatchingFilter(FriendRowViewModel row)
    {
        string filter = FilterText.Trim();
        return filter.Length == 0 || row.Nickname.Contains(filter, StringComparison.OrdinalIgnoreCase);
    }

    private void HandleMessageRequested(object? sender, MessageRequestedEventArgs e)
    {
        Actions.CloseAll();
        SelectTab(FriendsTab.Chat);
        Chat.OpenPrivate(e.Friend);
    }

    private void HandleFriendPresenceChanged(object? sender, FriendPresenceChangedEventArgs e)
    {
        Action updateFriendPresence = () => UpdateFriendPresence(e);
        UiThread.Run(_uiContext, updateFriendPresence);
    }

    private void UpdateFriendPresence(FriendPresenceChangedEventArgs change)
    {
        int index = _allRows.FindIndex(row => row.FriendCode == change.Friend.Friend.FriendCode);
        if (index < 0)
        {
            return;
        }

        _allRows[index] = new FriendRowViewModel(change.Friend, change.OccurredAt);
        RefreshFriendGroups();
        Chat.RefreshPresence(change.Friend.Friend.FriendCode);
    }

    private void HandleFriendRequestReceived(object? sender, FriendRequestReceivedEventArgs e)
    {
        Action receiveFriendRequest = () => ReceiveFriendRequest(e.Request);
        UiThread.Run(_uiContext, receiveFriendRequest);
    }

    private void ReceiveFriendRequest(PendingFriendRequest request)
    {
        bool isRequestsTabShown = IsOpen && IsRequestsTab;
        Requests.ReceiveRequest(request, !isRequestsTabShown);
    }

    private void HandleRemoteFriendshipStarted(object? sender, FriendshipStartedEventArgs e)
    {
        UiThread.Run(_uiContext, ReloadFriendsAndRequests);
    }

    private void ReloadFriendsAndRequests()
    {
        _ = RunSafely(LoadFriends());
        _ = RunSafely(Requests.LoadRequests());
    }

    private void ShowListing(FriendListing listing)
    {
        _allRows.Clear();
        foreach (ListedFriend friend in listing.Friends)
        {
            var row = new FriendRowViewModel(friend, listing.ListedAt);
            _allRows.Add(row);
        }

        _formattedFriendCode = FriendCodePolicy.Format(listing.OwnFriendCode);
        OwnFriendCodeText = FormatText(Strings.FriendsOwnCodeFormat, _formattedFriendCode);
        RefreshFriendGroups();
    }

    private void ReportLoadFailure(Exception cause)
    {
        _logger.Error("The friend list could not be loaded.", cause);
        HasLoadFailed = true;
    }
}
