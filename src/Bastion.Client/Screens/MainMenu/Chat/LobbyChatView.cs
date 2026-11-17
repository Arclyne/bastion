using System;
using System.Collections.Specialized;
using System.ComponentModel;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;
using Gum.Wireframe;
using log4net;

namespace Bastion.Client.Screens.MainMenu.Chat;

public sealed class LobbyChatView : StackPanel
{
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
        Width = ConversationPartFactory.ViewWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Spacing = PanelMetrics.SectionSpacing;
        _messages = CreateMessages();
        StackPanel header = CreateHeader();
        Label systemNotice = ConversationPartFactory.CreateWrappedLabel(
            nameof(Strings.ChatSystemNotice),
            TextStyleCatalog.Hint);
        Label emptyLabel = CreateEmptyLabel();
        Button unreadButton = CreateUnreadButton();
        Label muteNotice = ConversationPartFactory.CreateNoticeLabel(
            nameof(LobbyChatViewModel.MuteText),
            nameof(LobbyChatViewModel.IsMuted),
            TextStyleCatalog.FieldError);
        StackPanel composer = ConversationPartFactory.CreateComposer(HandleSendClick);

        AddChild(header);
        AddChild(systemNotice);
        AddChild(emptyLabel);
        AddChild(_messages);
        AddChild(unreadButton);
        AddChild(muteNotice);
        AddChild(composer);

        _chat.Messages.CollectionChanged += HandleMessagesChanged;
        _chat.PropertyChanged += HandleChatPropertyChanged;
    }

    private ItemsControl CreateMessages()
    {
        ItemsControl messages = ConversationPartFactory.CreateMessageList(ScrollHeight, RowSpacing);
        messages.VisualTemplate = ChatMessageRowView.CreateTemplate(ConversationPartFactory.MessageRowWidth);
        messages.Items = _chat.Messages;
        var messagesVisual = (ScrollViewerVisual)messages.Visual;
        var scrollBarVisual = (InteractiveGue)messagesVisual.VerticalScrollBarInstance;
        if (scrollBarVisual.FormsControlAsObject is ScrollBar scrollBar)
        {
            scrollBar.ValueChanged += HandleScrollChanged;
        }

        return messages;
    }

    private void HandleScrollChanged(object? sender, EventArgs e)
    {
        bool isReadingLatest = _messages.VerticalScrollBarValue >= _messages.VerticalScrollBarMaximum - LatestTolerance;
        _chat.UpdateReadingPosition(isReadingLatest);
    }

    private StackPanel CreateHeader()
    {
        Label title = ConversationPartFactory.CreateGlobalChannelTitle();
        Label detail = ConversationPartFactory.CreateGlobalChannelDetail();
        return ConversationPartFactory.CreateHeader(HandleBackClick, title, detail);
    }

    private void HandleBackClick(object? sender, EventArgs e)
    {
        _chat.CloseConversation();
    }

    private static Label CreateEmptyLabel()
    {
        return ViewFactory.CreateToggledLabel(
            nameof(Strings.ChatEmpty),
            nameof(LobbyChatViewModel.IsEmpty),
            TextStyleCatalog.NoticeTitle);
    }

    private Button CreateUnreadButton()
    {
        Button unread = ViewFactory.CreateBoundUntranslatedButton(
            nameof(LobbyChatViewModel.UnreadText),
            UnreadButtonWidth,
            UnreadButtonHeight);
        ButtonSkin.CreateAndAttach(unread, ButtonLookCatalog.Pill);
        unread.SetBinding(nameof(IsVisible), nameof(LobbyChatViewModel.HasUnread));
        unread.Click += HandleUnreadClick;
        return unread;
    }

    private void HandleUnreadClick(object? sender, EventArgs e)
    {
        _chat.UpdateReadingPosition(isReadingLatest: true);
        ConversationPartFactory.ScrollToLatest(_messages);
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
            ConversationPartFactory.ScrollToLatest(_messages);
        }
    }

    private void HandleChatPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LobbyChatViewModel.IsConversationOpen) && _chat.IsConversationOpen)
        {
            ConversationPartFactory.ScrollToLatest(_messages);
        }
    }
}
