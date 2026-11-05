using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Accounts;
using Bastion.Client.Networking.Chat;
using Bastion.Client.Networking.Friends;
using Bastion.Client.Networking.PrivateChat;
using Bastion.Client.Networking.Sessions;
using Bastion.Domain.Chat;
using Gum.Mvvm;
using log4net;

namespace Bastion.Client.Screens.MainMenu.Chat;

public sealed class PrivateConversationViewModel : ViewModel
{
    private const int CounterVisibleFromLength = 180;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(PrivateConversationViewModel));

    private readonly SignedInSession _session;
    private readonly IConversationClient _conversations;
    private PlayerTag _partner = new PlayerTag(string.Empty, string.Empty);
    private DateTime? _lastDay;

    public PrivateConversationViewModel(SignedInSession session, IConversationClient conversations)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(conversations);

        _session = session;
        _conversations = conversations;
        PartnerNickname = string.Empty;
        PresenceText = string.Empty;
        GreetingText = string.Empty;
        OfflineText = string.Empty;
        MuteText = string.Empty;
        StatusText = string.Empty;
        CounterText = string.Empty;
        Draft = string.Empty;
        IsFriend = true;
    }

    public event EventHandler<ConversationUpdatedEventArgs>? MessageSent;

    public event EventHandler? Closed;

    public ObservableCollection<PrivateMessageRowViewModel> Messages { get; } =
        new ObservableCollection<PrivateMessageRowViewModel>();

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
    public bool CanWrite => IsFriend && !IsMuted && !IsLoading && !HasLoadFailed;

    public bool IsSending
    {
        get => Get<bool>();
        private set => Set(value);
    }

    [DependsOn(nameof(CanWrite))]
    [DependsOn(nameof(IsSending))]
    public bool CanSend => CanWrite && !IsSending;

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

    public string StatusText
    {
        get => Get<string>();
        private set => Set(value);
    }

    [DependsOn(nameof(StatusText))]
    public bool HasStatus => StatusText.Length > 0;

    public bool IsShowing(string friendCode)
    {
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
            ReportLoadFailure(ex);
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
        if (!ChatRules.IsValidMessage(text))
        {
            StatusText = ChatRejectionText.DescribeInvalidMessage();
            return;
        }

        PlayerTag partner = _partner;
        var draftMessage = new PrivateMessage(text.Trim(), DateTime.UtcNow, true);
        PrivateMessageRowViewModel row = AddMessage(draftMessage, isPending: true);
        Draft = string.Empty;
        IsSending = true;
        try
        {
            DateTime sentAt = await Task.Run(
                () => _conversations.SendMessage(_session.Token, partner.FriendCode, text));
            row.Confirm(sentAt);
            MessageSent?.Invoke(this, new ConversationUpdatedEventArgs(partner, draftMessage with { SentAt = sentAt }));
        }
        catch (ChatMessageRejectedException ex)
        {
            _logger.Info($"The server turned down the private message. Rejection={ex.Rejection}");
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

    private static string FormatText(string format, params object[] arguments)
    {
        return string.Format(CultureInfo.CurrentCulture, format, arguments);
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
            ReportMarkReadFailure(ex);
        }
        catch (InvalidSessionException ex)
        {
            ReportMarkReadFailure(ex);
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
            ApplyPresence(null);
        }

        IsMuted = transcript.Mute.IsMuted;
        MuteText = ChatRejectionText.DescribeMute(transcript.Mute);
    }

    private void ClearMessages()
    {
        Messages.Clear();
        _lastDay = null;
        IsEmpty = true;
    }

    private PrivateMessageRowViewModel AddMessage(PrivateMessage message, bool isPending)
    {
        DateTime day = ChatTimeText.ToLocalDay(message.SentAt);
        string dayLabel = day == _lastDay ? string.Empty : ChatTimeText.DescribeDay(message.SentAt, DateTime.Now.Date);
        _lastDay = day;
        PrivateMessageRowViewModel row = isPending
            ? PrivateMessageRowViewModel.CreatePending(message, dayLabel)
            : new PrivateMessageRowViewModel(message, dayLabel);
        Messages.Add(row);
        IsEmpty = false;
        return row;
    }

    private void ShowRejection(PrivateMessageRowViewModel row, ChatMessageRejectedException rejection)
    {
        row.Fail(ChatRejectionText.Describe(rejection.Rejection));
        if (rejection.Rejection == ChatRejection.Muted)
        {
            IsMuted = true;
            MuteText = ChatRejectionText.DescribeMute(new ChatMute(true, rejection.MutedUntil));
        }

        if (rejection.Rejection == ChatRejection.NotFriends)
        {
            IsFriend = false;
            ApplyPresence(null);
        }
    }

    private void UpdateCounter()
    {
        int length = Draft.Length;
        IsCounterVisible = length >= CounterVisibleFromLength;
        CounterText = FormatText(Strings.ChatCounterFormat, length, ChatRules.MaximumMessageLength);
    }

    private void ReportLoadFailure(Exception ex)
    {
        _logger.Warn("The private conversation could not be loaded.", ex);
        HasLoadFailed = true;
    }

    private void ReportSendFailure(PrivateMessageRowViewModel row, Exception ex)
    {
        _logger.Warn("The private message could not be sent.", ex);
        row.Fail(Strings.ChatSendFailed);
    }

    private void ReportMarkReadFailure(Exception ex)
    {
        _logger.Warn("The private conversation could not be marked as read.", ex);
    }
}
