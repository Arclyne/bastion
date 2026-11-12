using System;
using System.Collections.Specialized;
using System.ComponentModel;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;
using Gum.Wireframe;
using log4net;

namespace Bastion.Client.Screens.MainMenu.Chat;

public sealed class LobbyChatView : StackPanel
{
    private const float SectionSpacing = 10;
    private const float ScrollHeight = 300;
    private const float RowSpacing = 12;
    private const float UnreadButtonWidth = 200;
    private const float UnreadButtonHeight = 32;
    private const float LatestTolerance = 1;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(LobbyChatView));

    private readonly LobbyChatViewModel _chat;
    private readonly ItemsControl _messages;

    public LobbyChatView(LobbyChatViewModel chat)
    {
        ArgumentNullException.ThrowIfNull(chat);

        _chat = chat;
        BindingContext = chat;
        Width = ConversationParts.ViewWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Spacing = SectionSpacing;
        _messages = CreateMessages();

        AddChild(CreateHeader());
        AddChild(ConversationParts.CreateWrappedLabel(nameof(Strings.ChatSystemNotice), TextStyles.Hint));
        AddChild(CreateEmptyLabel());
        AddChild(_messages);
        AddChild(CreateUnreadButton());
        AddChild(ConversationParts.CreateNoticeLabel(
            nameof(LobbyChatViewModel.MuteText),
            nameof(LobbyChatViewModel.IsMuted),
            TextStyles.FieldError));
        AddChild(ConversationParts.CreateNoticeLabel(
            nameof(LobbyChatViewModel.StatusText),
            nameof(LobbyChatViewModel.HasStatus),
            TextStyles.FieldError));
        AddChild(ConversationParts.CreateNoticeLabel(
            nameof(LobbyChatViewModel.CounterText),
            nameof(LobbyChatViewModel.IsCounterVisible),
            TextStyles.Hint));
        AddChild(ConversationParts.CreateInputRow(HandleSendClick, ConversationParts.ViewWidth));

        _chat.Messages.CollectionChanged += HandleMessagesChanged;
        _chat.PropertyChanged += HandleChatPropertyChanged;
    }

    private ItemsControl CreateMessages()
    {
        ItemsControl messages = ConversationParts.CreateMessageList(ScrollHeight, RowSpacing);
        messages.VisualTemplate = new VisualTemplate(ChatMessageRowView.CreateVisual);
        messages.Items = _chat.Messages;
        var messagesVisual = (ScrollViewerVisual)messages.Visual;
        var scrollBarVisual = (InteractiveGue)messagesVisual.VerticalScrollBarInstance;
        if (scrollBarVisual.FormsControlAsObject is ScrollBar scrollBar)
        {
            scrollBar.ValueChanged += HandleScrollChanged;
        }

        return messages;
    }

    private StackPanel CreateHeader()
    {
        Label title = ViewFactory.CreateLabel(nameof(Strings.ChatGlobalChannel), TextStyles.IdentityName);
        Label detail = ViewFactory.CreateBoundUntranslatedLabel(nameof(LobbyChatViewModel.ChannelDetail), TextStyles.Hint);
        return ConversationParts.CreateHeader(HandleBackClick, title, detail);
    }

    private static Label CreateEmptyLabel()
    {
        Label empty = ViewFactory.CreateLabel(nameof(Strings.ChatEmpty), TextStyles.NoticeTitle);
        empty.SetBinding(nameof(IsVisible), nameof(LobbyChatViewModel.IsEmpty));
        return empty;
    }

    private Button CreateUnreadButton()
    {
        Button unread = ViewFactory.CreateButton(string.Empty, UnreadButtonWidth, UnreadButtonHeight);
        ButtonSkin.CreateAndAttach(unread, ButtonLooks.Pill);
        var unreadVisual = (ButtonVisual)unread.Visual;
        unreadVisual.TextInstance.LocalizeText = false;
        unread.SetBinding(nameof(Button.Text), nameof(LobbyChatViewModel.UnreadText));
        unread.SetBinding(nameof(IsVisible), nameof(LobbyChatViewModel.HasUnread));
        unread.Click += HandleUnreadClick;
        return unread;
    }

    private void HandleScrollChanged(object? sender, EventArgs e)
    {
        bool isReadingLatest = _messages.VerticalScrollBarValue >= _messages.VerticalScrollBarMaximum - LatestTolerance;
        _chat.UpdateReadingPosition(isReadingLatest);
    }

    private void HandleBackClick(object? sender, EventArgs e)
    {
        _chat.CloseConversation();
    }

    private void HandleUnreadClick(object? sender, EventArgs e)
    {
        _chat.UpdateReadingPosition(isReadingLatest: true);
        ConversationParts.ScrollToLatest(_messages);
    }

    private async void HandleSendClick(object? sender, EventArgs e)
    {
        try
        {
            await _chat.SendMessage();
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("The chat message could not be sent because the connection is closed.", ex);
        }
    }

    private void HandleMessagesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_chat.IsReadingLatest)
        {
            ConversationParts.ScrollToLatest(_messages);
        }
    }

    private void HandleChatPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LobbyChatViewModel.IsConversationOpen) && _chat.IsConversationOpen)
        {
            ConversationParts.ScrollToLatest(_messages);
        }
    }
}
