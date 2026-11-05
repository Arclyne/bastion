using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using Bastion.Client.Controls;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;
using Gum.Wireframe;
using log4net;

namespace Bastion.Client.Screens.MainMenu.Chat;

public sealed class ChatConversationView : StackPanel
{
    private const float ViewWidth = 360;
    private const float SectionSpacing = 10;
    private const float HeaderSpacing = 12;
    private const float TextSpacing = 2;
    private const float BackButtonWidth = 96;
    private const float BackButtonHeight = 36;
    private const float ScrollHeight = 300;
    private const float RowSpacing = 12;
    private const float UnreadButtonWidth = 200;
    private const float UnreadButtonHeight = 32;
    private const float InputHeight = 44;
    private const float SendButtonWidth = 96;
    private const float InputSpacing = 8;
    private const float LatestTolerance = 1;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(ChatConversationView));

    private readonly LobbyChatViewModel _chat;
    private readonly ScrollViewer _messages;

    public ChatConversationView(LobbyChatViewModel chat)
    {
        ArgumentNullException.ThrowIfNull(chat);

        _chat = chat;
        Width = ViewWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Spacing = SectionSpacing;
        _messages = CreateMessages();

        AddChild(CreateHeader());
        AddChild(CreateWrappedLabel(nameof(Strings.ChatSystemNotice), TextStyles.Hint));
        AddChild(CreateEmptyLabel());
        AddChild(_messages);
        AddChild(CreateUnreadButton());
        AddChild(CreateBoundLabel(
            nameof(LobbyChatViewModel.MuteText),
            nameof(LobbyChatViewModel.IsMuted),
            TextStyles.FieldError));
        AddChild(CreateBoundLabel(
            nameof(LobbyChatViewModel.StatusText),
            nameof(LobbyChatViewModel.HasStatus),
            TextStyles.FieldError));
        AddChild(CreateBoundLabel(
            nameof(LobbyChatViewModel.CounterText),
            nameof(LobbyChatViewModel.IsCounterVisible),
            TextStyles.Hint));
        AddChild(CreateInputRow());

        _chat.Messages.CollectionChanged += HandleMessagesChanged;
        _chat.PropertyChanged += HandleChatPropertyChanged;
    }

    private static Label CreateWrappedLabel(string textKey, TextStyle style)
    {
        Label label = ViewFactory.CreateLabel(textKey, style);
        label.Width = ViewWidth;
        label.WidthUnits = DimensionUnitType.Absolute;
        return label;
    }

    private static Label CreateBoundLabel(string textProperty, string visibilityProperty, TextStyle style)
    {
        Label label = CreateWrappedLabel(string.Empty, style);
        ((LabelVisual)label.Visual).LocalizeText = false;
        label.SetBinding(nameof(Label.Text), textProperty);
        label.SetBinding(nameof(IsVisible), visibilityProperty);
        return label;
    }

    private static Label CreateEmptyLabel()
    {
        Label empty = ViewFactory.CreateLabel(nameof(Strings.ChatEmpty), TextStyles.NoticeTitle);
        empty.SetBinding(nameof(IsVisible), nameof(LobbyChatViewModel.IsEmpty));
        return empty;
    }

    private StackPanel CreateHeader()
    {
        Button back = ViewFactory.CreateButton(nameof(Strings.ChatBack), BackButtonWidth, BackButtonHeight);
        ButtonSkin.CreateAndAttach(back, ButtonLooks.Pill);
        back.Click += HandleBackClick;

        Label detail = ViewFactory.CreateLabel(string.Empty, TextStyles.Hint);
        ((LabelVisual)detail.Visual).LocalizeText = false;
        detail.SetBinding(nameof(Label.Text), nameof(LobbyChatViewModel.ChannelDetail));

        var title = new StackPanel { Spacing = TextSpacing };
        title.AddChild(ViewFactory.CreateLabel(nameof(Strings.ChatGlobalChannel), TextStyles.IdentityName));
        title.AddChild(detail);

        var header = new StackPanel { Orientation = Orientation.Horizontal, Spacing = HeaderSpacing };
        header.AddChild(back);
        header.AddChild(title);
        return header;
    }

    private ScrollViewer CreateMessages()
    {
        var messages = new ScrollViewer
        {
            Width = ViewWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Height = ScrollHeight,
            HeightUnits = DimensionUnitType.Absolute,
        };
        Shapes.HideDefaultParts(messages);
        messages.InnerPanel.StackSpacing = RowSpacing;
        var scrollBarVisual = (InteractiveGue)((ScrollViewerVisual)messages.Visual).VerticalScrollBarInstance;
        if (scrollBarVisual.FormsControlAsObject is ScrollBar scrollBar)
        {
            scrollBar.ValueChanged += HandleScrollChanged;
        }
        return messages;
    }

    private Button CreateUnreadButton()
    {
        Button unread = ViewFactory.CreateButton(string.Empty, UnreadButtonWidth, UnreadButtonHeight);
        ButtonSkin.CreateAndAttach(unread, ButtonLooks.Pill);
        ((ButtonVisual)unread.Visual).TextInstance.LocalizeText = false;
        unread.SetBinding(nameof(Button.Text), nameof(LobbyChatViewModel.UnreadText));
        unread.SetBinding(nameof(IsVisible), nameof(LobbyChatViewModel.HasUnread));
        unread.Click += HandleUnreadClick;
        return unread;
    }

    private StackPanel CreateInputRow()
    {
        var input = new TextField
        {
            Placeholder = nameof(Strings.ChatPlaceholder),
            Width = ViewWidth - SendButtonWidth - InputSpacing,
            Height = InputHeight,
        };
        input.SetBinding(nameof(TextField.Text), nameof(LobbyChatViewModel.Draft));
        input.SetBinding(nameof(IsEnabled), nameof(LobbyChatViewModel.CanWrite));

        Button send = ViewFactory.CreateButton(nameof(Strings.ChatSend), SendButtonWidth, InputHeight);
        ButtonSkin.CreateAndAttach(send, ButtonLooks.Primary);
        send.SetBinding(nameof(IsEnabled), nameof(LobbyChatViewModel.CanSend));
        send.Click += HandleSendClick;

        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = InputSpacing };
        row.AddChild(input);
        row.AddChild(send);
        return row;
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
            default:
                RebuildRows();
                break;
        }

        if (_chat.IsReadingLatest)
        {
            ScrollToLatest();
        }
    }

    private void AddRows(IList? rows)
    {
        if (rows is null)
        {
            return;
        }

        foreach (ChatMessageRowViewModel row in rows)
        {
            _messages.InnerPanel.AddChild(new ChatMessageRowView(row).Visual);
        }
    }

    private void RebuildRows()
    {
        while (_messages.InnerPanel.Children.Count > 0)
        {
            _messages.InnerPanel.Children.RemoveAt(_messages.InnerPanel.Children.Count - 1);
        }

        AddRows(_chat.Messages);
    }

    private void ScrollToLatest()
    {
        _messages.VerticalScrollBarValue = _messages.VerticalScrollBarMaximum;
    }

    private void HandleChatPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LobbyChatViewModel.IsConversationOpen) && _chat.IsConversationOpen)
        {
            ScrollToLatest();
        }
    }

    private void HandleScrollChanged(object? sender, EventArgs e)
    {
        bool isReadingLatest = _messages.VerticalScrollBarValue >= _messages.VerticalScrollBarMaximum - LatestTolerance;
        _chat.UpdateReadingPosition(isReadingLatest);
    }

    private void HandleUnreadClick(object? sender, EventArgs e)
    {
        _chat.UpdateReadingPosition(true);
        ScrollToLatest();
    }

    private void HandleBackClick(object? sender, EventArgs e)
    {
        _chat.CloseConversation();
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
}
