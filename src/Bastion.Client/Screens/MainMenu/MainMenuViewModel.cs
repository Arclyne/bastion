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

namespace Bastion.Client.Screens.MainMenu;

public sealed class MainMenuViewModel : ViewModel
{
    private const int MaximumChatLines = 50;

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
        _notifications.ChatMessageReceived += OnChatMessageReceived;
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
        catch (Exception ex) when (IsChatFailure(ex))
        {
            ChatStatusKey = nameof(Strings.MainMenuChatUnavailable);
        }
    }

    public async Task SendChatMessage()
    {
        if (IsSending)
        {
            return;
        }

        string text = ChatDraft;
        if (!ChatRules.IsMessage(text))
        {
            ChatStatusKey = nameof(Strings.MainMenuChatInvalid);
            return;
        }

        IsSending = true;
        try
        {
            await Task.Run(() => _chat.Send(_session.Token, text));
            ChatDraft = string.Empty;
        }
        catch (InvalidChatMessageException)
        {
            ChatStatusKey = nameof(Strings.MainMenuChatInvalid);
        }
        catch (Exception ex) when (IsChatFailure(ex))
        {
            ChatStatusKey = nameof(Strings.MainMenuChatUnavailable);
        }
        finally
        {
            IsSending = false;
        }
    }

    private static bool IsChatFailure(Exception ex)
    {
        return ex is InvalidSessionException
            || ex is ServerUnreachableException
            || ex is ConnectionLostException
            || ex is ServerErrorException;
    }

    private void OnChatMessageReceived(object? sender, ChatMessage message)
    {
        string line = $"{message.SenderNickname}: {message.Text}";
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
