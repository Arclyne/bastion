using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Chat;
using Bastion.Client.Networking.Notifications;
using Bastion.Client.Networking.Sessions;
using Bastion.Domain.Chat;
using Gum.Mvvm;
using log4net;

namespace Bastion.Client.Screens.MainMenu.Chat;

public sealed class LobbyChatViewModel : ModeratedChatViewModel, IDisposable
{
    private const int SingleMessage = 1;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(LobbyChatViewModel));

    private readonly LoggedInSession _session;
    private readonly IChatClient _chat;
    private readonly INotificationClient _notifications;
    private readonly SynchronizationContext? _uiContext;
    private int _unreadCount;

    public LobbyChatViewModel(ChatContext context)
        : base(GetTimeProvider(context))
    {
        _session = context.Session;
        _chat = context.Clients.Lobby;
        _notifications = context.Clients.Notifications;
        _uiContext = SynchronizationContext.Current;
        Messages = new ObservableCollection<ChatMessageRowViewModel>();
        ChannelDetail = string.Empty;
        UnreadText = string.Empty;
        IsReadingLatest = true;
        IsEmpty = true;
        _notifications.ChatMessageReceived += HandleChatMessageReceived;
    }

    public ObservableCollection<ChatMessageRowViewModel> Messages { get; }

    public bool IsConversationOpen
    {
        get => Get<bool>();
        private set => Set(value);
    }

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

    [DependsOn(nameof(IsMuted))]
    public override bool CanWrite => !IsMuted;

    [DependsOn(nameof(IsSending))]
    [DependsOn(nameof(IsMuted))]
    public override bool CanSend => !IsSending && !IsMuted;

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
        UpdateReadingPosition(isReadingLatest: true);
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
            _logger.Warn("The lobby chat could not be loaded because the session is no longer valid.", ex);
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

    public async Task SendMessage()
    {
        if (!CanSend)
        {
            return;
        }

        if (!TryTakeValidDraft(out string trimmedText))
        {
            return;
        }

        DateTime createdAt = GetUtcNow();
        var row = ChatMessageRowViewModel.CreatePending(trimmedText, createdAt, TimeProvider);
        UpdateReadingPosition(isReadingLatest: true);
        AddMessage(row);
        Draft = string.Empty;
        IsSending = true;
        try
        {
            DateTime sentAt = await Task.Run(() => _chat.SendLobbyChatMessage(_session.Token, trimmedText));
            row.Confirm(sentAt);
        }
        catch (ChatMessageRejectedException ex)
        {
            _logger.Warn($"The server turned down the chat message. Rejection={ex.Rejection}", ex);
            ShowRejection(row, ex.Rejection, ex.MutedUntil);
        }
        catch (InvalidSessionException ex)
        {
            _logger.Warn("The chat message could not be sent because the session is no longer valid.", ex);
            row.Fail(Strings.ChatSendFailed);
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

    private void HandleChatMessageReceived(object? sender, ChatMessageReceivedEventArgs e)
    {
        var row = new ChatMessageRowViewModel(e.Message, TimeProvider);
        UiThread.Run(_uiContext, () => ReceiveMessage(row));
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

    private void AddMessage(ChatMessageRowViewModel row)
    {
        AddRecentMessage(Messages, row);
        IsEmpty = false;
    }

    private void ShowTranscript(LobbyChatTranscript transcript)
    {
        Messages.Clear();
        foreach (ChatMessage message in transcript.Messages)
        {
            var row = new ChatMessageRowViewModel(message, TimeProvider);
            Messages.Add(row);
        }

        IsEmpty = Messages.Count == 0;
        ChannelDetail = FormatText(Strings.ChatConnectedCountFormat, transcript.ConnectedPlayerCount);
        ShowMute(transcript.Mute);
    }

    private void ReportLoadFailure(Exception cause)
    {
        _logger.Error("The lobby chat could not be loaded.", cause);
        HasLoadFailed = true;
    }

    private void ShowRejection(ChatMessageRowViewModel row, ChatRejection rejection, DateTime? mutedUntil)
    {
        row.Fail(ChatRejectionText.Describe(rejection));
        if (rejection == ChatRejection.Muted)
        {
            var mute = new ChatMute(IsMuted: true, mutedUntil);
            ShowMute(mute);
        }
    }

    private void ReportSendFailure(ChatMessageRowViewModel row, Exception cause)
    {
        _logger.Error("The chat message could not be sent.", cause);
        row.Fail(Strings.ChatSendFailed);
    }
}
