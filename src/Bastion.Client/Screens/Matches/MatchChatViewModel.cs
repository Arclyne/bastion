using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Bastion.Client.Localization;
using Bastion.Client.Networking.Channels;
using Bastion.Client.Networking.Chat;
using Bastion.Client.Networking.Notifications;
using Bastion.Client.Networking.Sessions;
using Bastion.Client.Screens.MainMenu.Chat;
using Bastion.Domain.Chat;
using Gum.Mvvm;
using log4net;

namespace Bastion.Client.Screens.Matches;

public sealed class MatchChatViewModel : ViewModel, IDisposable
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(MatchChatViewModel));

    private readonly JoinedMatch _joinedMatch;
    private readonly IChatClient _chat;
    private readonly TimeProvider _timeProvider;
    private readonly SynchronizationContext? _uiContext;

    public MatchChatViewModel(JoinedMatch joinedMatch, IChatClient chat, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(joinedMatch);
        ArgumentNullException.ThrowIfNull(chat);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _joinedMatch = joinedMatch;
        _chat = chat;
        _timeProvider = timeProvider;
        _uiContext = SynchronizationContext.Current;
        Draft = string.Empty;
        StatusText = string.Empty;
        _chat.MatchChatMessageReceived += HandleMatchChatMessageReceived;
    }

    public ObservableCollection<ChatMessageRowViewModel> Messages { get; } =
        new ObservableCollection<ChatMessageRowViewModel>();

    public string Draft
    {
        get => Get<string>();
        set
        {
            if (Set(value))
            {
                StatusText = string.Empty;
            }
        }
    }

    public bool IsSending
    {
        get => Get<bool>();
        private set => Set(value);
    }

    [DependsOn(nameof(IsSending))]
    public bool CanSend => !IsSending;

    public string StatusText
    {
        get => Get<string>();
        private set => Set(value);
    }

    [DependsOn(nameof(StatusText))]
    public bool HasStatus => StatusText.Length > 0;

    public async Task SendMessage()
    {
        if (IsSending)
        {
            return;
        }

        string text = Draft;
        if (!ChatPolicy.IsValidMessage(text))
        {
            StatusText = ChatRejectionText.DescribeInvalidMessage();
            return;
        }

        var row = ChatMessageRowViewModel.CreatePending(text.Trim(), _timeProvider.GetUtcNow().UtcDateTime);
        AddMessage(row);
        Draft = string.Empty;
        IsSending = true;
        try
        {
            DateTime sentAt = await Task.Run(() => SendToJoinedMatch(text));
            row.Confirm(sentAt);
        }
        catch (ChatMessageRejectedException ex)
        {
            _logger.Warn($"The server turned down a match chat message. Rejection={ex.Rejection}", ex);
            row.Fail(ChatRejectionText.Describe(ex.Rejection));
        }
        catch (ChatChannelForbiddenException ex)
        {
            ReportSendFailure(row, ex);
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
        _chat.MatchChatMessageReceived -= HandleMatchChatMessageReceived;
    }

    private static void ReportSendFailure(ChatMessageRowViewModel row, Exception ex)
    {
        _logger.Warn("A match chat message could not be sent.", ex);
        row.Fail(Strings.MatchChatUnavailable);
    }

    private DateTime SendToJoinedMatch(string text)
    {
        return _chat.SendMatchChatMessage(_joinedMatch.Session.Token, _joinedMatch.FoundMatch.MatchId, text);
    }

    private void AddMessage(ChatMessageRowViewModel row)
    {
        Messages.Add(row);
        if (Messages.Count > ChatPolicy.MaximumRecentMessages)
        {
            Messages.RemoveAt(0);
        }
    }

    private void HandleMatchChatMessageReceived(object? sender, MatchChatMessageReceivedEventArgs e)
    {
        if (e.MatchId != _joinedMatch.FoundMatch.MatchId)
        {
            return;
        }

        var row = new ChatMessageRowViewModel(e.Message);
        UiThread.Run(_uiContext, () => AddMessage(row));
    }
}
