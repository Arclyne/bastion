using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
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
    private readonly SynchronizationContext? _uiContext;

    public ChatTabViewModel(LoggedInSession session, ChatClients clients, IFriendDirectory friends)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(clients);
        ArgumentNullException.ThrowIfNull(friends);

        _session = session;
        _conversations = clients.Conversations;
        _friends = friends;
        _notifications = clients.Notifications;
        _uiContext = SynchronizationContext.Current;
        Lobby = new LobbyChatViewModel(session, clients.Lobby, clients.Notifications);
        Lobby.PropertyChanged += HandleLobbyPropertyChanged;
        Private = new PrivateConversationViewModel(session, clients.Conversations);
        Private.MessageSent += HandlePrivateMessageSent;
        Private.Closed += HandlePrivateClosed;
        _notifications.PrivateMessageReceived += HandlePrivateMessageReceived;
        UnreadText = string.Empty;
        Pane = ChatPane.List;
    }

    public LobbyChatViewModel Lobby { get; }

    public PrivateConversationViewModel Private { get; }

    public ObservableCollection<ConversationRowViewModel> Conversations { get; } =
        new ObservableCollection<ConversationRowViewModel>();

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
        await Task.WhenAll(Lobby.LoadChannel(), LoadConversations());
    }

    public async Task LoadConversations()
    {
        HasConversationsLoadFailed = false;
        try
        {
            IReadOnlyList<ConversationPreview> previews = await Task.Run(
                () => _conversations.ListConversations(_session.Token));
            ShowPreviews(previews.Select(preview => new ConversationRowViewModel(preview)));
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

    public void OpenGlobal()
    {
        Lobby.OpenConversation();
        Pane = ChatPane.Global;
    }

    public void OpenPrivate(PlayerTag partner)
    {
        ArgumentNullException.ThrowIfNull(partner);

        Pane = ChatPane.Private;
        ConversationRowViewModel? row = FindRow(partner.FriendCode);
        row?.UpdateUnread(NoUnread);
        UpdateUnreadTotal();
        _ = Private.Open(partner, _friends.FindPresence(partner.FriendCode));
    }

    public void RefreshPresence(string friendCode)
    {
        if (Private.IsShowing(friendCode))
        {
            Private.ApplyPresence(_friends.FindPresence(friendCode));
        }
    }

    public void BackToList()
    {
        Lobby.CloseConversation();
        if (Private.IsOpen)
        {
            Private.Close();
        }

        Pane = ChatPane.List;
    }

    public void Dispose()
    {
        _notifications.PrivateMessageReceived -= HandlePrivateMessageReceived;
        Lobby.Dispose();
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

    private ConversationRowViewModel? FindRow(string friendCode)
    {
        return Conversations.FirstOrDefault(row => row.Partner.FriendCode == friendCode);
    }

    private void UpdateConversation(PlayerTag partner, PrivateMessage message, int newUnread)
    {
        ConversationRowViewModel? row = FindRow(partner.FriendCode);
        if (row is null)
        {
            var preview = new ConversationPreview(new ConversationContact(partner, true), message, newUnread);
            Conversations.Insert(FirstPosition, new ConversationRowViewModel(preview));
        }
        else
        {
            row.ShowMessage(message, row.UnreadCount + newUnread);
            Conversations.Move(Conversations.IndexOf(row), FirstPosition);
        }

        HasConversations = true;
        UpdateUnreadTotal();
    }

    private void UpdateUnreadTotal()
    {
        int total = Conversations.Sum(row => row.UnreadCount);
        UnreadText = total > NoUnread ? total.ToString(CultureInfo.CurrentCulture) : string.Empty;
    }

    private void HandleLobbyPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LobbyChatViewModel.IsConversationOpen)
            && !Lobby.IsConversationOpen
            && Pane == ChatPane.Global)
        {
            Pane = ChatPane.List;
        }
    }

    private void HandlePrivateClosed(object? sender, EventArgs e)
    {
        Pane = ChatPane.List;
    }

    private void HandlePrivateMessageSent(object? sender, ConversationUpdatedEventArgs e)
    {
        UpdateConversation(e.Partner, e.Message, NoUnread);
    }

    private void HandlePrivateMessageReceived(object? sender, PrivateMessageReceivedEventArgs e)
    {
        RunOnUi(() => ReceivePrivateMessage(e.Sender, e.Message));
    }

    private void ReceivePrivateMessage(PlayerTag sender, PrivateMessage message)
    {
        if (Private.IsShowing(sender.FriendCode))
        {
            Private.Receive(message);
            UpdateConversation(sender, message, NoUnread);
            return;
        }

        UpdateConversation(sender, message, OneUnread);
    }

    private void RunOnUi(Action action)
    {
        if (_uiContext is null)
        {
            action();
            return;
        }

        _uiContext.Post(state => ((Action)state!).Invoke(), action);
    }

    private void ReportLoadFailure(Exception ex)
    {
        _logger.Warn("The private conversations could not be loaded.", ex);
        HasConversationsLoadFailed = true;
    }
}
