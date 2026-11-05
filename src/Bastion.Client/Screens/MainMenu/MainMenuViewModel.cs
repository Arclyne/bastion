using System;
using System.Collections.ObjectModel;
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

namespace Bastion.Client.Screens.MainMenu;

public sealed class MainMenuViewModel : ViewModel
{
    private const int MaximumChatLines = 50;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(MainMenuViewModel));

    private readonly SignedInSession _session;
    private readonly INotificationClient _notifications;
    private readonly IChatClient _chat;
    private readonly SynchronizationContext? _uiContext;

    public MainMenuViewModel(SignedInSession session, INotificationClient notifications, IChatClient chat)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(notifications);
        ArgumentNullException.ThrowIfNull(chat);

        _session = session;
        _notifications = notifications;
        _chat = chat;
        _uiContext = SynchronizationContext.Current;
        Nickname = session.Nickname;
        ChatDraft = string.Empty;
        ChatStatusKey = string.Empty;
        _notifications.ChatMessageReceived += HandleChatMessageReceived;
    }

    public string Nickname
    {
        get => Get<string>();
        private set => Set(value);
    }

    public ObservableCollection<string> ChatLines { get; } = new ObservableCollection<string>();

    public string ChatDraft
    {
        get => Get<string>();
        set
        {
            if (Set(value))
            {
                ChatStatusKey = string.Empty;
            }
        }
    }

    public string ChatStatusKey
    {
        get => Get<string>();
        private set => Set(value);
    }

    [DependsOn(nameof(ChatStatusKey))]
    public bool HasChatStatus => ChatStatusKey.Length > 0;

    public bool IsSending
    {
        get => Get<bool>();
        private set => Set(value);
    }

    [DependsOn(nameof(IsSending))]
    public bool CanSend => !IsSending;

    public async Task ConnectChat()
    {
        try
        {
            await Task.Run(() => _notifications.Connect(_session.Token));
        }
        catch (InvalidSessionException ex)
        {
            ReportConnectFailure(ex);
        }
        catch (ServerUnreachableException ex)
        {
            ReportConnectFailure(ex);
        }
        catch (ConnectionLostException ex)
        {
            ReportConnectFailure(ex);
        }
        catch (ServerErrorException ex)
        {
            ReportConnectFailure(ex);
        }
    }

    public async Task SendChatMessage()
    {
        if (IsSending)
        {
            return;
        }

        string text = ChatDraft;
        if (!ChatRules.IsValidMessage(text))
        {
            ChatStatusKey = nameof(Strings.MainMenuChatInvalid);
            return;
        }

        IsSending = true;
        try
        {
            await Task.Run(() => _chat.SendMessage(_session.Token, text));
            ChatDraft = string.Empty;
        }
        catch (InvalidChatMessageException ex)
        {
            _logger.Warn("The server turned down the chat message.", ex);
            ChatStatusKey = nameof(Strings.MainMenuChatInvalid);
        }
        catch (InvalidSessionException ex)
        {
            ReportSendFailure(ex);
        }
        catch (ServerUnreachableException ex)
        {
            ReportSendFailure(ex);
        }
        catch (ConnectionLostException ex)
        {
            ReportSendFailure(ex);
        }
        catch (ServerErrorException ex)
        {
            ReportSendFailure(ex);
        }
        finally
        {
            IsSending = false;
        }
    }

    private void ReportConnectFailure(Exception ex)
    {
        _logger.Warn("The lobby chat could not be connected.", ex);
        ShowChatUnavailable();
    }

    private void ReportSendFailure(Exception ex)
    {
        _logger.Warn("The chat message could not be sent.", ex);
        ShowChatUnavailable();
    }

    private void ShowChatUnavailable()
    {
        ChatStatusKey = nameof(Strings.MainMenuChatUnavailable);
    }

    private void HandleChatMessageReceived(object? sender, ChatMessageReceivedEventArgs e)
    {
        string line = $"{e.SenderNickname}: {e.Text}";
        if (_uiContext is null)
        {
            AddChatLine(line);
            return;
        }

        _uiContext.Post(state => AddChatLine((string)state!), line);
    }

    private void AddChatLine(string line)
    {
        ChatLines.Add(line);
        if (ChatLines.Count > MaximumChatLines)
        {
            ChatLines.RemoveAt(0);
        }
    }
}
