using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Friends;
using Bastion.Client.Networking.Notifications;
using Bastion.Client.Networking.PrivateChat;
using Bastion.Client.Networking.Sessions;
using Gum.Mvvm;
using log4net;

namespace Bastion.Client.Screens.MainMenu.Chat;

public sealed class ChatTabViewModel : ViewModel, IDisposable
{
    private const int FirstPosition = 0;
    private const int NoUnread = 0;
    private const int OneUnread = 1;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(ChatTabViewModel));

    private readonly LoggedInSession _session;
    private readonly IConversationClient _conversations;
    private readonly IFriendDirectory _friends;
    private readonly INotificationClient _notifications;
    private readonly TimeProvider _timeProvider;
    private readonly SynchronizationContext? _uiContext;

    public ChatTabViewModel(ChatContext context, IFriendDirectory friends)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(friends);

        _session = context.Session;
        _conversations = context.Clients.Conversations;
        _friends = friends;
        _notifications = context.Clients.Notifications;
        _timeProvider = context.TimeProvider;
        _uiContext = SynchronizationContext.Current;
        LobbyChat = new LobbyChatViewModel(context);
        LobbyChat.PropertyChanged += HandleLobbyPropertyChanged;
        PrivateConversation = new PrivateConversationViewModel(context);
        PrivateConversation.MessageSent += HandlePrivateMessageSent;
        PrivateConversation.Closed += HandlePrivateClosed;
        _notifications.PrivateMessageReceived += HandlePrivateMessageReceived;
        Conversations = new ObservableCollection<ConversationRowViewModel>();
        UnreadText = string.Empty;
        Pane = ChatPane.List;
    }

    public LobbyChatViewModel LobbyChat { get; }

    public PrivateConversationViewModel PrivateConversation { get; }

    public ObservableCollection<ConversationRowViewModel> Conversations { get; }

    public ChatPane Pane
    {
        get => Get<ChatPane>();
        private set => Set(value);
    }

    [DependsOn(nameof(Pane))]
    public bool IsListVisible => Pane == ChatPane.List;

    [DependsOn(nameof(Pane))]
    public bool IsGlobalOpen => Pane == ChatPane.Global;

    [DependsOn(nameof(Pane))]
    public bool IsPrivateOpen => Pane == ChatPane.Private;

    public bool HasConversations
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public bool HasConversationsLoadFailed
    {
        get => Get<bool>();
        private set => Set(value);
    }

    [DependsOn(nameof(HasConversations))]
    [DependsOn(nameof(HasConversationsLoadFailed))]
    public bool HasNoConversations => !HasConversations && !HasConversationsLoadFailed;

    public string UnreadText
    {
        get => Get<string>();
        private set => Set(value);
    }

    [DependsOn(nameof(UnreadText))]
    public bool HasUnread => UnreadText.Length > 0;

    public async Task Load()
    {
        await Task.WhenAll(LobbyChat.LoadChannel(), LoadConversations());
    }

    public async Task LoadConversations()
    {
        HasConversationsLoadFailed = false;
        try
        {
            IReadOnlyList<ConversationPreview> previews = await Task.Run(
                () => _conversations.ListConversations(_session.Token));
            DateTime today = ChatTimeText.GetLocalToday(_timeProvider);
            IEnumerable<ConversationRowViewModel> rows = previews.Select(
                preview => new ConversationRowViewModel(preview, today));
            ShowPreviews(rows);
        }
        catch (InvalidSessionException ex)
        {
            _logger.Warn("The private conversations could not be loaded because the session is no longer valid.", ex);
            HasConversationsLoadFailed = true;
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

    public void OpenGlobal()
    {
        LobbyChat.OpenConversation();
        Pane = ChatPane.Global;
    }

    public void OpenPrivate(PlayerTag partner)
    {
        ArgumentNullException.ThrowIfNull(partner);

        Pane = ChatPane.Private;
        ConversationRowViewModel? row = FindRow(partner.FriendCode);
        row?.UpdateUnread(NoUnread);
        UpdateUnreadTotal();
        _ = PrivateConversation.Open(partner, _friends.FindPresence(partner.FriendCode));
    }

    public void RefreshPresence(string friendCode)
    {
        ArgumentNullException.ThrowIfNull(friendCode);

        if (!PrivateConversation.IsShowing(friendCode))
        {
            return;
        }

        PrivateConversation.ApplyPresence(_friends.FindPresence(friendCode));
    }

    public void BackToList()
    {
        LobbyChat.CloseConversation();
        if (PrivateConversation.IsOpen)
        {
            PrivateConversation.Close();
        }

        Pane = ChatPane.List;
    }

    public void Dispose()
    {
        _notifications.PrivateMessageReceived -= HandlePrivateMessageReceived;
        LobbyChat.Dispose();
    }

    private void ShowPreviews(IEnumerable<ConversationRowViewModel> rows)
    {
        Conversations.Clear();
        foreach (ConversationRowViewModel row in rows)
        {
            Conversations.Add(row);
        }

        HasConversations = Conversations.Count > 0;
        UpdateUnreadTotal();
    }

    private void UpdateUnreadTotal()
    {
        int total = Conversations.Sum(row => row.UnreadCount);
        UnreadText = BadgeText.FormatCount(total);
    }

    private void ReportLoadFailure(Exception ex)
    {
        _logger.Error("The private conversations could not be loaded.", ex);
        HasConversationsLoadFailed = true;
    }

    private ConversationRowViewModel? FindRow(string friendCode)
    {
        return Conversations.FirstOrDefault(row => row.Partner.FriendCode == friendCode);
    }

    private void UpdateConversation(PlayerTag partner, PrivateMessage message, int newUnread)
    {
        ConversationRowViewModel? row = FindRow(partner.FriendCode);
        DateTime today = ChatTimeText.GetLocalToday(_timeProvider);
        if (row is null)
        {
            var contact = new ConversationContact(partner, isFriend: true);
            var preview = new ConversationPreview(contact, message, newUnread);
            var newRow = new ConversationRowViewModel(preview, today);
            Conversations.Insert(FirstPosition, newRow);
        }
        else
        {
            row.ShowMessage(message, row.UnreadCount + newUnread, today);
            Conversations.Move(Conversations.IndexOf(row), FirstPosition);
        }

        HasConversations = true;
        UpdateUnreadTotal();
    }

    private void ReceivePrivateMessage(PlayerTag sender, PrivateMessage message)
    {
        if (PrivateConversation.IsShowing(sender.FriendCode))
        {
            PrivateConversation.Receive(message);
            UpdateConversation(sender, message, NoUnread);
            return;
        }

        UpdateConversation(sender, message, OneUnread);
    }

    private void HandleLobbyPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LobbyChatViewModel.IsConversationOpen)
            && !LobbyChat.IsConversationOpen
            && Pane == ChatPane.Global)
        {
            Pane = ChatPane.List;
        }
    }

    private void HandlePrivateMessageSent(object? sender, ConversationUpdatedEventArgs e)
    {
        UpdateConversation(e.Partner, e.Message, NoUnread);
    }

    private void HandlePrivateClosed(object? sender, EventArgs e)
    {
        Pane = ChatPane.List;
    }

    private void HandlePrivateMessageReceived(object? sender, PrivateMessageReceivedEventArgs e)
    {
        UiThread.Run(_uiContext, () => ReceivePrivateMessage(e.Sender, e.Message));
    }
}
