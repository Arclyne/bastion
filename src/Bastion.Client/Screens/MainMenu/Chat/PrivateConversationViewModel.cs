using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Chat;
using Bastion.Client.Networking.Friends;
using Bastion.Client.Networking.PrivateChat;
using Bastion.Client.Networking.Sessions;
using Gum.Mvvm;
using log4net;

namespace Bastion.Client.Screens.MainMenu.Chat;

public sealed class PrivateConversationViewModel : ModeratedChatViewModel
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(PrivateConversationViewModel));

    private readonly LoggedInSession _session;
    private readonly IConversationClient _conversations;
    private PlayerTag _partner;
    private DateTime? _lastDay;
    private bool _isOpen;

    public PrivateConversationViewModel(ChatContext context)
        : base(GetTimeProvider(context))
    {
        _session = context.Session;
        _conversations = context.Clients.Conversations;
        _partner = new PlayerTag(string.Empty, string.Empty);
        Messages = new ObservableCollection<PrivateMessageRowViewModel>();
        PartnerNickname = string.Empty;
        PresenceText = string.Empty;
        GreetingText = string.Empty;
        OfflineText = string.Empty;
        IsFriend = true;
    }

    public ObservableCollection<PrivateMessageRowViewModel> Messages { get; }

    public string PartnerNickname
    {
        get => Get<string>();
        private set => Set(value);
    }

    public string PresenceText
    {
        get => Get<string>();
        private set => Set(value);
    }

    private bool IsPartnerConnected
    {
        get => Get<bool>();
        set => Set(value);
    }

    private bool IsFriend
    {
        get => Get<bool>();
        set => Set(value);
    }

    public bool IsLoading
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public bool HasLoadFailed
    {
        get => Get<bool>();
        private set => Set(value);
    }

    private bool IsEmpty
    {
        get => Get<bool>();
        set => Set(value);
    }

    [DependsOn(nameof(IsEmpty))]
    [DependsOn(nameof(IsLoading))]
    public bool IsGreetingVisible => IsEmpty && !IsLoading;

    public string GreetingText
    {
        get => Get<string>();
        private set => Set(value);
    }

    public string OfflineText
    {
        get => Get<string>();
        private set => Set(value);
    }

    [DependsOn(nameof(IsFriend))]
    [DependsOn(nameof(IsPartnerConnected))]
    [DependsOn(nameof(IsLoading))]
    public bool IsOfflineNoticeVisible => IsFriend && !IsPartnerConnected && !IsLoading;

    [DependsOn(nameof(IsFriend))]
    [DependsOn(nameof(IsLoading))]
    public bool IsNotFriendsNoticeVisible => !IsFriend && !IsLoading;

    [DependsOn(nameof(IsFriend))]
    [DependsOn(nameof(IsMuted))]
    [DependsOn(nameof(IsLoading))]
    public bool IsMuteNoticeVisible => IsMuted && IsFriend && !IsLoading;

    [DependsOn(nameof(IsFriend))]
    [DependsOn(nameof(IsMuted))]
    [DependsOn(nameof(IsLoading))]
    [DependsOn(nameof(HasLoadFailed))]
    public override bool CanWrite => IsFriend && !IsMuted && !IsLoading && !HasLoadFailed;

    [DependsOn(nameof(CanWrite))]
    [DependsOn(nameof(IsSending))]
    public override bool CanSend => CanWrite && !IsSending;

    public event EventHandler<PrivateMessageSentEventArgs>? MessageSent;

    public event EventHandler? Closed;

    public bool IsShowing(string friendCode)
    {
        ArgumentNullException.ThrowIfNull(friendCode);

        return _isOpen && string.Equals(_partner.FriendCode, friendCode, StringComparison.Ordinal);
    }

    public async Task Open(PlayerTag partner, ConversationPresence presence)
    {
        ArgumentNullException.ThrowIfNull(partner);
        ArgumentNullException.ThrowIfNull(presence);

        _partner = partner;
        _isOpen = true;
        PartnerNickname = partner.Nickname;
        GreetingText = FormatText(Strings.PrivateGreetingFormat, partner.Nickname);
        OfflineText = FormatText(Strings.PrivateOfflineFormat, partner.Nickname);
        IsFriend = presence.Presence != PartnerPresence.NotFriend;
        ApplyPresence(presence);
        Draft = string.Empty;
        ClearMessages();
        await Load();
    }

    public void Close()
    {
        _isOpen = false;
        OnClosed();
    }

    public void ApplyPresence(ConversationPresence presence)
    {
        ArgumentNullException.ThrowIfNull(presence);

        IsPartnerConnected = presence.Presence == PartnerPresence.Connected;
        PresenceText = presence.StatusText;
    }

    public void Receive(PrivateMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        AddMessage(message, isPending: false);
        _ = RunSafely(MarkRead());
    }

    public async Task Load()
    {
        HasLoadFailed = false;
        IsLoading = true;
        string friendCode = _partner.FriendCode;
        try
        {
            ConversationTranscript transcript = await Task.Run(
                () => _conversations.OpenConversation(_session.Token, friendCode));
            ShowTranscript(transcript);
        }
        catch (ChatMessageRejectedException ex)
        {
            _logger.Warn($"The server turned down the private conversation. Rejection={ex.Rejection}", ex);
            ShowLoadFailure();
        }
        catch (InvalidSessionException ex)
        {
            _logger.Warn("The private conversation could not be loaded because the session is no longer valid.", ex);
            ShowLoadFailure();
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
        finally
        {
            IsLoading = false;
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

        PlayerTag partner = _partner;
        DateTime createdAt = GetUtcNow();
        var draftMessage = new PrivateMessage(trimmedText, createdAt, isOwn: true);
        PrivateMessageRowViewModel row = AddMessage(draftMessage, isPending: true);
        Draft = string.Empty;
        IsSending = true;
        try
        {
            DateTime sentAt = await Task.Run(
                () => _conversations.SendMessage(_session.Token, partner.FriendCode, trimmedText));
            row.Confirm(sentAt);
            PrivateMessage sentMessage = draftMessage with { SentAt = sentAt };
            var messageSent = new PrivateMessageSentEventArgs(partner, sentMessage);
            OnMessageSent(messageSent);
        }
        catch (ChatMessageRejectedException ex)
        {
            _logger.Warn($"The server turned down the private message. Rejection={ex.Rejection}", ex);
            ShowRejection(row, ex.Rejection, ex.MutedUntil);
        }
        catch (InvalidSessionException ex)
        {
            _logger.Warn("The private message could not be sent because the session is no longer valid.", ex);
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

    private void ClearMessages()
    {
        Messages.Clear();
        _lastDay = null;
        IsEmpty = true;
    }

    private void OnClosed()
    {
        Closed?.Invoke(this, EventArgs.Empty);
    }

    private PrivateMessageRowViewModel AddMessage(PrivateMessage message, bool isPending)
    {
        DateTime day = ChatTimeText.GetLocalDay(message.SentAt, TimeProvider);
        DateTime today = GetLocalToday();
        string dayText = ChatTimeText.DescribeDay(message.SentAt, today, TimeProvider);
        string dayLabel = day == _lastDay ? string.Empty : dayText;
        _lastDay = day;
        PrivateMessageRowViewModel row = isPending
            ? PrivateMessageRowViewModel.CreatePending(message, dayLabel, TimeProvider)
            : new PrivateMessageRowViewModel(message, dayLabel, TimeProvider);
        Messages.Add(row);
        IsEmpty = false;
        return row;
    }

    private async Task MarkRead()
    {
        string friendCode = _partner.FriendCode;
        try
        {
            await Task.Run(() => _conversations.MarkConversationRead(_session.Token, friendCode));
        }
        catch (ChatMessageRejectedException ex)
        {
            _logger.Warn(
                $"The server turned down marking the private conversation as read. Rejection={ex.Rejection}",
                ex);
        }
        catch (InvalidSessionException ex)
        {
            _logger.Warn(
                "The private conversation could not be marked as read because the session is no longer valid.",
                ex);
        }
        catch (ServerUnreachableException ex)
        {
            ReportMarkReadFailure(ex);
        }
        catch (ConnectionLostException ex)
        {
            ReportMarkReadFailure(ex);
        }
        catch (ServerErrorException ex)
        {
            ReportMarkReadFailure(ex);
        }
    }

    private void ReportMarkReadFailure(Exception cause)
    {
        _logger.Error("The private conversation could not be marked as read.", cause);
    }

    private static async Task RunSafely(Task task)
    {
        try
        {
            await task;
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("Marking the conversation read ended because the connection was closed.", ex);
        }
    }

    private void ShowTranscript(ConversationTranscript transcript)
    {
        ClearMessages();
        foreach (PrivateMessage message in transcript.Messages)
        {
            AddMessage(message, isPending: false);
        }

        IsFriend = transcript.Contact.IsFriend;
        if (!IsFriend)
        {
            var nonFriendPresence = ConversationPresence.CreateForNonFriend();
            ApplyPresence(nonFriendPresence);
        }

        ShowMute(transcript.Mute);
    }

    private void ReportLoadFailure(Exception cause)
    {
        _logger.Error("The private conversation could not be loaded.", cause);
        ShowLoadFailure();
    }

    private void ShowLoadFailure()
    {
        HasLoadFailed = true;
    }

    private void OnMessageSent(PrivateMessageSentEventArgs e)
    {
        MessageSent?.Invoke(this, e);
    }

    private void ShowRejection(PrivateMessageRowViewModel row, ChatRejection rejection, DateTime? mutedUntil)
    {
        row.Fail(ChatRejectionText.Describe(rejection));
        if (rejection == ChatRejection.Muted)
        {
            var mute = new ChatMute(IsMuted: true, mutedUntil);
            ShowMute(mute);
        }

        if (rejection == ChatRejection.NotFriends)
        {
            IsFriend = false;
            var nonFriendPresence = ConversationPresence.CreateForNonFriend();
            ApplyPresence(nonFriendPresence);
        }
    }

    private void ReportSendFailure(PrivateMessageRowViewModel row, Exception cause)
    {
        _logger.Error("The private message could not be sent.", cause);
        row.Fail(Strings.ChatSendFailed);
    }
}
