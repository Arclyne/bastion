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
    private readonly ItemsControl _messagesItemsControl;

    public LobbyChatView(LobbyChatViewModel chat)
    {
        ArgumentNullException.ThrowIfNull(chat);

        _chat = chat;
        BindingContext = chat;
        Width = ConversationPartFactory.ViewWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Spacing = PanelMetrics.SectionSpacing;
        _messagesItemsControl = CreateMessages();
        StackPanel headerStackPanel = CreateHeader();
        Label systemNoticeLabel = ConversationPartFactory.CreateWrappedLabel(
            nameof(Strings.ChatSystemNotice),
            TextStyleCatalog.Hint);
        Label emptyLabel = CreateEmptyLabel();
        Button unreadButton = CreateUnreadButton();
        Label muteNoticeLabel = ConversationPartFactory.CreateNoticeLabel(
            nameof(LobbyChatViewModel.MuteText),
            nameof(LobbyChatViewModel.IsMuted),
            TextStyleCatalog.FieldError);
        StackPanel composerStackPanel = ConversationPartFactory.CreateComposer(HandleSendClick);

        AddChild(headerStackPanel);
        AddChild(systemNoticeLabel);
        AddChild(emptyLabel);
        AddChild(_messagesItemsControl);
        AddChild(unreadButton);
        AddChild(muteNoticeLabel);
        AddChild(composerStackPanel);

        _chat.Messages.CollectionChanged += HandleMessagesChanged;
        _chat.PropertyChanged += HandleChatPropertyChanged;
    }

    private ItemsControl CreateMessages()
    {
        ItemsControl messagesItemsControl = ConversationPartFactory.CreateMessageList(ScrollHeight, RowSpacing);
        messagesItemsControl.VisualTemplate = ChatMessageRowView.CreateTemplate(
            ConversationPartFactory.MessageRowWidth);
        messagesItemsControl.Items = _chat.Messages;
        var messagesVisual = (ScrollViewerVisual)messagesItemsControl.Visual;
        var scrollBarVisual = (InteractiveGue)messagesVisual.VerticalScrollBarInstance;
        if (scrollBarVisual.FormsControlAsObject is ScrollBar scrollBar)
        {
            scrollBar.ValueChanged += HandleScrollChanged;
        }

        return messagesItemsControl;
    }

    private void HandleScrollChanged(object? sender, EventArgs e)
    {
        bool isReadingLatest = _messagesItemsControl.VerticalScrollBarValue
            >= _messagesItemsControl.VerticalScrollBarMaximum - LatestTolerance;
        _chat.UpdateReadingPosition(isReadingLatest);
    }

    private StackPanel CreateHeader()
    {
        Label titleLabel = ConversationPartFactory.CreateGlobalChannelTitle();
        Label detailLabel = ConversationPartFactory.CreateGlobalChannelDetail();
        return ConversationPartFactory.CreateHeader(HandleBackClick, titleLabel, detailLabel);
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
        Button unreadButton = ViewFactory.CreateBoundUntranslatedButton(
            nameof(LobbyChatViewModel.UnreadText),
            UnreadButtonWidth,
            UnreadButtonHeight);
        ButtonSkin.CreateAndAttach(unreadButton, ButtonLookCatalog.Pill);
        unreadButton.SetBinding(nameof(IsVisible), nameof(LobbyChatViewModel.HasUnread));
        unreadButton.Click += HandleUnreadClick;
        return unreadButton;
    }

    private void HandleUnreadClick(object? sender, EventArgs e)
    {
        _chat.UpdateReadingPosition(isReadingLatest: true);
        ConversationPartFactory.ScrollToLatest(_messagesItemsControl);
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
            ConversationPartFactory.ScrollToLatest(_messagesItemsControl);
        }
    }

    private void HandleChatPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LobbyChatViewModel.IsConversationOpen) && _chat.IsConversationOpen)
        {
            ConversationPartFactory.ScrollToLatest(_messagesItemsControl);
        }
    }
}
