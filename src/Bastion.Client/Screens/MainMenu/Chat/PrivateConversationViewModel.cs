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
using Bastion.Domain.Chat;
using Gum.Mvvm;
using log4net;

namespace Bastion.Client.Screens.MainMenu.Chat;

public sealed class PrivateConversationViewModel : ChatComposerViewModel
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(PrivateConversationViewModel));

    private readonly LoggedInSession _session;
    private readonly IConversationClient _conversations;
    private PlayerTag _partner = new PlayerTag(string.Empty, string.Empty);
    private DateTime? _lastDay;

    public PrivateConversationViewModel(ChatContext context)
        : base(GetTimeProvider(context))
    {
        _session = context.Session;
        _conversations = context.Clients.Conversations;
        Messages = new ObservableCollection<PrivateMessageRowViewModel>();
        PartnerNickname = string.Empty;
        PresenceText = string.Empty;
        GreetingText = string.Empty;
        OfflineText = string.Empty;
        MuteText = string.Empty;
        IsFriend = true;
    }

    public ObservableCollection<PrivateMessageRowViewModel> Messages { get; }

    public bool IsOpen { get; private set; }

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

    public bool IsPartnerConnected
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public bool IsFriend
    {
        get => Get<bool>();
        private set => Set(value);
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

    public bool IsEmpty
    {
        get => Get<bool>();
        private set => Set(value);
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

    public bool IsMuted
    {
        get => Get<bool>();
        private set => Set(value);
    }

    public string MuteText
    {
        get => Get<string>();
        private set => Set(value);
    }

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

    public event EventHandler<ConversationUpdatedEventArgs>? MessageSent;

    public event EventHandler? Closed;

    public bool IsShowing(string friendCode)
    {
        ArgumentNullException.ThrowIfNull(friendCode);

        return IsOpen && string.Equals(_partner.FriendCode, friendCode, StringComparison.Ordinal);
    }

    public async Task Open(PlayerTag partner, ConversationPresence? presence)
    {
        ArgumentNullException.ThrowIfNull(partner);

        _partner = partner;
        IsOpen = true;
        PartnerNickname = partner.Nickname;
        GreetingText = FormatText(Strings.PrivateGreetingFormat, partner.Nickname);
        OfflineText = FormatText(Strings.PrivateOfflineFormat, partner.Nickname);
        IsFriend = presence is not null;
        ApplyPresence(presence);
        Draft = string.Empty;
        ClearMessages();
        await Load();
    }

    public void Close()
    {
        IsOpen = false;
        Closed?.Invoke(this, EventArgs.Empty);
    }

    public void ApplyPresence(ConversationPresence? presence)
    {
        IsPartnerConnected = presence?.IsConnected ?? false;
        PresenceText = presence?.StatusText ?? Strings.PrivateStatusNotFriend;
    }

    public void Receive(PrivateMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        AddMessage(message, isPending: false);
        _ = MarkRead();
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
            ReportLoadRejection(ex);
        }
        catch (InvalidSessionException ex)
        {
            ReportLoadRejection(ex);
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

        string text = Draft;
        if (!ChatPolicy.IsValidMessage(text))
        {
            StatusText = ChatRejectionText.DescribeInvalidMessage();
            return;
        }

        PlayerTag partner = _partner;
        string trimmedText = text.Trim();
        DateTime createdAt = GetUtcNow();
        var draftMessage = new PrivateMessage(trimmedText, createdAt, isOwn: true);
        PrivateMessageRowViewModel row = AddMessage(draftMessage, isPending: true);
        Draft = string.Empty;
        IsSending = true;
        try
        {
            DateTime sentAt = await Task.Run(
                () => _conversations.SendMessage(_session.Token, partner.FriendCode, text));
            row.Confirm(sentAt);
            PrivateMessage sentMessage = draftMessage with { SentAt = sentAt };
            var update = new ConversationUpdatedEventArgs(partner, sentMessage);
            MessageSent?.Invoke(this, update);
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

    private PrivateMessageRowViewModel AddMessage(PrivateMessage message, bool isPending)
    {
        DateTime day = ChatTimeText.GetLocalDay(message.SentAt);
        DateTime today = GetLocalToday();
        string dayLabel = day == _lastDay ? string.Empty : ChatTimeText.DescribeDay(message.SentAt, today);
        _lastDay = day;
        PrivateMessageRowViewModel row = isPending
            ? PrivateMessageRowViewModel.CreatePending(message, dayLabel)
            : new PrivateMessageRowViewModel(message, dayLabel);
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
            ReportMarkReadRejection(ex);
        }
        catch (InvalidSessionException ex)
        {
            ReportMarkReadRejection(ex);
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

    private void ReportMarkReadRejection(Exception ex)
    {
        _logger.Warn("The server turned down marking the private conversation as read.", ex);
    }

    private void ReportMarkReadFailure(Exception ex)
    {
        _logger.Error("The private conversation could not be marked as read.", ex);
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
            ApplyPresence(presence: null);
        }

        IsMuted = transcript.Mute.IsMuted;
        MuteText = ChatRejectionText.DescribeMute(transcript.Mute);
    }

    private void ReportLoadRejection(Exception ex)
    {
        _logger.Warn("The server turned down the private conversation.", ex);
        HasLoadFailed = true;
    }

    private void ReportLoadFailure(Exception ex)
    {
        _logger.Error("The private conversation could not be loaded.", ex);
        HasLoadFailed = true;
    }

    private void ShowRejection(PrivateMessageRowViewModel row, ChatRejection rejection, DateTime? mutedUntil)
    {
        row.Fail(ChatRejectionText.Describe(rejection));
        if (rejection == ChatRejection.Muted)
        {
            var mute = new ChatMute(IsMuted: true, mutedUntil);
            IsMuted = true;
            MuteText = ChatRejectionText.DescribeMute(mute);
        }

        if (rejection == ChatRejection.NotFriends)
        {
            IsFriend = false;
            ApplyPresence(presence: null);
        }
    }

    private void ReportSendFailure(PrivateMessageRowViewModel row, Exception ex)
    {
        _logger.Error("The private message could not be sent.", ex);
        row.Fail(Strings.ChatSendFailed);
    }
}
