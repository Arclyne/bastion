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

public sealed class MatchChatViewModel : ChatComposerViewModel, IDisposable
{
    private const int OldestMessageIndex = 0;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(MatchChatViewModel));

    private readonly JoinedMatch _joinedMatch;
    private readonly IChatClient _chat;
    private readonly SynchronizationContext? _uiContext;

    public MatchChatViewModel(JoinedMatch joinedMatch, IChatClient chat, TimeProvider timeProvider)
        : base(timeProvider)
    {
        ArgumentNullException.ThrowIfNull(joinedMatch);
        ArgumentNullException.ThrowIfNull(chat);

        _joinedMatch = joinedMatch;
        _chat = chat;
        _uiContext = SynchronizationContext.Current;
        Messages = new ObservableCollection<ChatMessageRowViewModel>();
        _chat.MatchChatMessageReceived += HandleMatchChatMessageReceived;
    }

    public ObservableCollection<ChatMessageRowViewModel> Messages { get; }

    public override bool CanWrite => true;

    [DependsOn(nameof(IsSending))]
    public override bool CanSend => !IsSending;

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

        string trimmedText = text.Trim();
        DateTime createdAt = GetUtcNow();
        var row = ChatMessageRowViewModel.CreatePending(trimmedText, createdAt);
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
            _logger.Warn("The match chat message was turned down because the player is not in the match.", ex);
            row.Fail(Strings.MatchChatUnavailable);
        }
        catch (InvalidSessionException ex)
        {
            _logger.Warn("The match chat message could not be sent because the session is no longer valid.", ex);
            row.Fail(Strings.MatchChatUnavailable);
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
        _logger.Error("A match chat message could not be sent.", ex);
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
            Messages.RemoveAt(OldestMessageIndex);
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
