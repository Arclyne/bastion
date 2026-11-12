using System;
using System.Collections;
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
    private const float SectionSpacing = 10;
    private const float ScrollHeight = 300;
    private const float RowSpacing = 12;
    private const float UnreadButtonWidth = 200;
    private const float UnreadButtonHeight = 32;
    private const float LatestTolerance = 1;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(LobbyChatView));

    private readonly LobbyChatViewModel _chat;
    private readonly ScrollViewer _messages;

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
        AddChild(ConversationParts.CreateInputRow(HandleSendClick));

        _chat.Messages.CollectionChanged += HandleMessagesChanged;
        _chat.PropertyChanged += HandleChatPropertyChanged;
    }

    private ScrollViewer CreateMessages()
    {
        ScrollViewer messages = ConversationParts.CreateMessageList(ScrollHeight, RowSpacing);
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

    private void AddRows(IList? rows)
    {
        if (rows is null)
        {
            return;
        }

        foreach (ChatMessageRowViewModel row in rows)
        {
            var rowView = new ChatMessageRowView(row);
            _messages.InnerPanel.AddChild(rowView.Visual);
        }
    }

    private void RebuildRows()
    {
        ViewFactory.ClearRows(_messages.InnerPanel);
        AddRows(_chat.Messages);
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
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                AddRows(e.NewItems);
                break;
            case NotifyCollectionChangedAction.Remove:
                _messages.InnerPanel.Children.RemoveAt(e.OldStartingIndex);
                break;
            case NotifyCollectionChangedAction.Replace:
            case NotifyCollectionChangedAction.Move:
            case NotifyCollectionChangedAction.Reset:
                RebuildRows();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(e), e.Action, "Unsupported collection change.");
        }

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
