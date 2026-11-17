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
    private const int NotFoundIndex = -1;
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
        PrivateConversation = new PrivateConversationViewModel(context);
        Conversations = new ObservableCollection<ConversationRowViewModel>();
        UnreadText = string.Empty;
        Pane = ChatPane.List;
        LobbyChat.PropertyChanged += HandleLobbyPropertyChanged;
        PrivateConversation.MessageSent += HandlePrivateMessageSent;
        PrivateConversation.Closed += HandlePrivateClosed;
        _notifications.PrivateMessageReceived += HandlePrivateMessageReceived;
    }

    public LobbyChatViewModel LobbyChat { get; }

    public PrivateConversationViewModel PrivateConversation { get; }

    public ObservableCollection<ConversationRowViewModel> Conversations { get; }

    private ChatPane Pane
    {
        get => Get<ChatPane>();
        set => Set(value);
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
                preview => new ConversationRowViewModel(preview, today, _timeProvider));
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
        int rowIndex = FindRowIndex(partner.FriendCode);
        if (rowIndex != NotFoundIndex)
        {
            Conversations[rowIndex].UpdateUnread(0);
        }

        UpdateUnreadTotal();
        ConversationPresence presence = _friends.GetPresence(partner.FriendCode);
        _ = RunSafely(PrivateConversation.Open(partner, presence));
    }

    public void RefreshPresence(string friendCode)
    {
        ArgumentNullException.ThrowIfNull(friendCode);

        if (!PrivateConversation.IsShowing(friendCode))
        {
            return;
        }

        ConversationPresence presence = _friends.GetPresence(friendCode);
        PrivateConversation.ApplyPresence(presence);
    }

    public void Dispose()
    {
        _notifications.PrivateMessageReceived -= HandlePrivateMessageReceived;
        LobbyChat.Dispose();
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

    private void HandlePrivateMessageSent(object? sender, PrivateMessageSentEventArgs e)
    {
        UpdateConversation(e.Partner, e.Message, 0);
    }

    private void UpdateConversation(PlayerTag partner, PrivateMessage message, int newUnread)
    {
        int rowIndex = FindRowIndex(partner.FriendCode);
        DateTime today = ChatTimeText.GetLocalToday(_timeProvider);
        if (rowIndex == NotFoundIndex)
        {
            var contact = new ConversationContact(partner, isFriend: true);
            var preview = new ConversationPreview(contact, message, newUnread);
            var newRow = new ConversationRowViewModel(preview, today, _timeProvider);
            Conversations.Insert(FirstPosition, newRow);
        }
        else
        {
            ConversationRowViewModel row = Conversations[rowIndex];
            row.ShowMessage(message, row.UnreadCount + newUnread, today);
            Conversations.Move(rowIndex, FirstPosition);
        }

        HasConversations = true;
        UpdateUnreadTotal();
    }

    private int FindRowIndex(string friendCode)
    {
        for (int rowIndex = 0; rowIndex < Conversations.Count; rowIndex++)
        {
            if (Conversations[rowIndex].Partner.FriendCode == friendCode)
            {
                return rowIndex;
            }
        }

        return NotFoundIndex;
    }

    private void UpdateUnreadTotal()
    {
        int total = Conversations.Sum(row => row.UnreadCount);
        UnreadText = BadgeText.FormatCount(total);
    }

    private void HandlePrivateClosed(object? sender, EventArgs e)
    {
        Pane = ChatPane.List;
    }

    private void HandlePrivateMessageReceived(object? sender, PrivateMessageReceivedEventArgs e)
    {
        Action receivePrivateMessage = () => ReceivePrivateMessage(e.Sender, e.Message);
        UiThread.Run(_uiContext, receivePrivateMessage);
    }

    private void ReceivePrivateMessage(PlayerTag sender, PrivateMessage message)
    {
        if (PrivateConversation.IsShowing(sender.FriendCode))
        {
            PrivateConversation.Receive(message);
            UpdateConversation(sender, message, 0);
            return;
        }

        UpdateConversation(sender, message, OneUnread);
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

    private void ReportLoadFailure(Exception cause)
    {
        _logger.Error("The private conversations could not be loaded.", cause);
        HasConversationsLoadFailed = true;
    }

    private static async Task RunSafely(Task task)
    {
        try
        {
            await task;
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("A private conversation ended because the connection was closed.", ex);
        }
    }
}
