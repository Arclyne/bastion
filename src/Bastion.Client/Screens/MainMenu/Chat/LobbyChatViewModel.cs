using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Networking.Chat;
using Bastion.Client.Networking.Notifications;
using Bastion.Client.Networking.Sessions;
using Bastion.Domain.Chat;
using Gum.Mvvm;
using log4net;

namespace Bastion.Client.Screens.MainMenu.Chat;

public sealed class LobbyChatViewModel : ViewModel, IDisposable
{
    private const int CounterVisibleFromLength = 180;
    private const int SingleMessage = 1;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(LobbyChatViewModel));

    private readonly SignedInSession _session;
    private readonly IChatClient _chat;
    private readonly INotificationClient _notifications;
    private readonly SynchronizationContext? _uiContext;
    private int _unreadCount;

    public LobbyChatViewModel(SignedInSession session, IChatClient chat, INotificationClient notifications)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(chat);
        ArgumentNullException.ThrowIfNull(notifications);

        _session = session;
        _chat = chat;
        _uiContext = SynchronizationContext.Current;
        _notifications = notifications;
        _notifications.ChatMessageReceived += HandleChatMessageReceived;
        Draft = string.Empty;
        ChannelDetail = string.Empty;
        CounterText = string.Empty;
        MuteText = string.Empty;
        StatusText = string.Empty;
        UnreadText = string.Empty;
        IsReadingLatest = true;
        IsEmpty = true;
    }

    public ObservableCollection<ChatMessageRowViewModel> Messages { get; } =
        new ObservableCollection<ChatMessageRowViewModel>();

    public bool IsConversationOpen
    {
        get => Get<bool>();
        private set => Set(value);
    }

    [DependsOn(nameof(IsConversationOpen))]
    public bool IsChannelListVisible => !IsConversationOpen;

    public string ChannelDetail
    {
        get => Get<string>();
        private set => Set(value);
    }

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

    public string Draft
    {
        get => Get<string>();
        set
        {
            if (Set(value))
            {
                StatusText = string.Empty;
                UpdateCounter();
            }
        }
    }

    public string CounterText
    {
        get => Get<string>();
        private set => Set(value);
    }

    public bool IsCounterVisible
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public bool IsMuted
    {
        get => Get<bool>();
        private set => Set(value);
    }

    [DependsOn(nameof(IsMuted))]
    public bool CanWrite => !IsMuted;

    public string MuteText
    {
        get => Get<string>();
        private set => Set(value);
    }

    public bool IsSending
    {
        get => Get<bool>();
        private set => Set(value);
    }

    [DependsOn(nameof(IsSending))]
    [DependsOn(nameof(IsMuted))]
    public bool CanSend => !IsSending && !IsMuted;

    public string StatusText
    {
        get => Get<string>();
        private set => Set(value);
    }

    [DependsOn(nameof(StatusText))]
    public bool HasStatus => StatusText.Length > 0;

    public bool IsReadingLatest { get; private set; }

    public string UnreadText
    {
        get => Get<string>();
        private set => Set(value);
    }

    [DependsOn(nameof(UnreadText))]
    public bool HasUnread => UnreadText.Length > 0;

    public void OpenConversation()
    {
        IsConversationOpen = true;
        UpdateReadingPosition(true);
    }

    public void CloseConversation()
    {
        IsConversationOpen = false;
    }

    public void UpdateReadingPosition(bool isReadingLatest)
    {
        IsReadingLatest = isReadingLatest;
        if (isReadingLatest)
        {
            _unreadCount = 0;
            UnreadText = string.Empty;
        }
    }

    public async Task LoadChannel()
    {
        HasLoadFailed = false;
        try
        {
            LobbyChatTranscript transcript = await Task.Run(() => _chat.OpenLobbyChat(_session.Token));
            ShowTranscript(transcript);
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

    public async Task SendMessage()
    {
        if (!CanSend)
        {
            return;
        }

        string text = Draft;
        if (!ChatRules.IsValidMessage(text))
        {
            StatusText = ChatRejectionText.DescribeInvalidMessage();
            return;
        }

        var row = ChatMessageRowViewModel.CreatePending(text.Trim(), DateTime.UtcNow);
        UpdateReadingPosition(true);
        AddMessage(row);
        Draft = string.Empty;
        IsSending = true;
        try
        {
            DateTime sentAt = await Task.Run(() => _chat.SendMessage(_session.Token, text));
            row.Confirm(sentAt);
        }
        catch (ChatMessageRejectedException ex)
        {
            _logger.Info($"The server turned down the chat message. Rejection={ex.Rejection}");
            ShowRejection(row, ex);
        }
        catch (InvalidSessionException ex)
        {
            ReportSendFailure(row, ex);
        }
        catch (ServerUnreachableException ex)
        {
            ReportSendFailure(row, ex);
        }
        catch (ConnectionLostException ex)
        {
            ReportSendFailure(row, ex);
        }
        catch (ServerErrorException ex)
        {
            ReportSendFailure(row, ex);
        }
        finally
        {
            IsSending = false;
        }
    }

    public void Dispose()
    {
        _notifications.ChatMessageReceived -= HandleChatMessageReceived;
    }

    private static string FormatText(string format, params object[] arguments)
    {
        return string.Format(CultureInfo.CurrentCulture, format, arguments);
    }

    private void ShowTranscript(LobbyChatTranscript transcript)
    {
        Messages.Clear();
        foreach (ChatMessage message in transcript.Messages)
        {
            Messages.Add(new ChatMessageRowViewModel(message));
        }

        IsEmpty = Messages.Count == 0;
        ChannelDetail = FormatText(Strings.ChatConnectedCountFormat, transcript.ConnectedPlayers);
        ShowMute(transcript.Mute);
    }

    private void ShowMute(ChatMute mute)
    {
        IsMuted = mute.IsMuted;
        MuteText = ChatRejectionText.DescribeMute(mute);
    }

    private void ShowRejection(ChatMessageRowViewModel row, ChatMessageRejectedException rejection)
    {
        row.Fail(ChatRejectionText.Describe(rejection.Rejection));
        if (rejection.Rejection == ChatRejection.Muted)
        {
            ShowMute(new ChatMute(true, rejection.MutedUntil));
        }
    }

    private void UpdateCounter()
    {
        int length = Draft.Length;
        IsCounterVisible = length >= CounterVisibleFromLength;
        CounterText = FormatText(Strings.ChatCounterFormat, length, ChatRules.MaximumMessageLength);
    }

    private void AddMessage(ChatMessageRowViewModel row)
    {
        Messages.Add(row);
        if (Messages.Count > ChatRules.RecentMessageCount)
        {
            Messages.RemoveAt(0);
        }

        IsEmpty = false;
    }

    private void HandleChatMessageReceived(object? sender, ChatMessageReceivedEventArgs e)
    {
        var row = new ChatMessageRowViewModel(e.Message);
        if (_uiContext is null)
        {
            ReceiveMessage(row);
            return;
        }

        _uiContext.Post(state => ReceiveMessage((ChatMessageRowViewModel)state!), row);
    }

    private void ReceiveMessage(ChatMessageRowViewModel row)
    {
        AddMessage(row);
        if (!IsConversationOpen || IsReadingLatest)
        {
            return;
        }

        _unreadCount++;
        UnreadText = _unreadCount == SingleMessage
            ? Strings.ChatNewMessage
            : FormatText(Strings.ChatNewMessagesFormat, _unreadCount);
    }

    private void ReportLoadFailure(Exception ex)
    {
        _logger.Warn("The lobby chat could not be loaded.", ex);
        HasLoadFailed = true;
    }

    private void ReportSendFailure(ChatMessageRowViewModel row, Exception ex)
    {
        _logger.Warn("The chat message could not be sent.", ex);
        row.Fail(Strings.ChatSendFailed);
    }
}
