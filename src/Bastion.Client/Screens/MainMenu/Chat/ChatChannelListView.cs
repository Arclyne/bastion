using System;
using System.Collections.Specialized;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;
using Gum.GueDeriving;
using log4net;

namespace Bastion.Client.Screens.MainMenu.Chat;

public sealed class ChatChannelListView : StackPanel
{
    private const float ListWidth = 360;
    private const float ListSpacing = 12;
    private const float RowHeight = 64;
    private const float RowRadius = 10;
    private const float RowPaddingX = 14;
    private const float TextSpacing = 2;
    private const float OpenButtonWidth = 88;
    private const float OpenButtonHeight = 36;
    private const float RetryButtonWidth = 140;
    private const float RetryButtonHeight = 40;
    private const float ConversationsHeight = 370;
    private const float ConversationSpacing = 8;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(ChatChannelListView));

    private readonly ChatTabViewModel _tab;
    private readonly ScrollViewer _conversations;

    public ChatChannelListView(ChatTabViewModel tab)
    {
        ArgumentNullException.ThrowIfNull(tab);

        _tab = tab;
        Width = ListWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Spacing = ListSpacing;
        _conversations = CreateConversations();
        AddChild(CreateGlobalRow());
        AddChild(CreateLobbyLoadFailure());
        AddChild(ViewFactory.CreateLabel(nameof(Strings.ChatWithFriendsTitle), TextStyles.SectionLabel));
        AddChild(CreateNoConversations());
        AddChild(CreateConversationsLoadFailure());
        AddChild(_conversations);

        _tab.Conversations.CollectionChanged += HandleConversationsChanged;
    }

    private static StackPanel CreateGlobalText()
    {
        Label detail = ViewFactory.CreateLabel(string.Empty, TextStyles.Hint);
        ((LabelVisual)detail.Visual).LocalizeText = false;
        detail.SetBinding(nameof(Label.Text), nameof(LobbyChatViewModel.ChannelDetail));

        var text = new StackPanel { Spacing = TextSpacing, X = RowPaddingX };
        text.AddChild(ViewFactory.CreateLabel(nameof(Strings.ChatGlobalChannel), TextStyles.IdentityName));
        text.AddChild(detail);
        ViewFactory.CenterVertically(text);
        return text;
    }

    private static StackPanel CreateNoConversations()
    {
        var empty = new StackPanel { Spacing = TextSpacing };
        empty.AddChild(ViewFactory.CreateLabel(nameof(Strings.ChatNoConversationsTitle), TextStyles.NoticeTitle));
        Label hint = ViewFactory.CreateLabel(nameof(Strings.ChatNoConversationsHint), TextStyles.Hint);
        hint.Width = ListWidth;
        hint.WidthUnits = DimensionUnitType.Absolute;
        empty.AddChild(hint);
        empty.SetBinding(nameof(IsVisible), nameof(ChatTabViewModel.HasNoConversations));
        return empty;
    }

    private static ScrollViewer CreateConversations()
    {
        var conversations = new ScrollViewer
        {
            Width = ListWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Height = ConversationsHeight,
            HeightUnits = DimensionUnitType.Absolute,
        };
        Shapes.HideDefaultParts(conversations);
        conversations.InnerPanel.StackSpacing = ConversationSpacing;
        conversations.SetBinding(nameof(IsVisible), nameof(ChatTabViewModel.HasConversations));
        return conversations;
    }

    private Panel CreateGlobalRow()
    {
        Button open = ViewFactory.CreateButton(nameof(Strings.ChatOpen), OpenButtonWidth, OpenButtonHeight);
        ButtonSkin.CreateAndAttach(open, ButtonLooks.Secondary);
        open.Anchor(Gum.Wireframe.Anchor.Right);
        open.X = -RowPaddingX;
        open.Click += HandleOpenGlobalClick;

        var row = new Panel
        {
            Width = ListWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Height = RowHeight,
            HeightUnits = DimensionUnitType.Absolute,
            BindingContext = _tab.Lobby,
        };
        RectangleRuntime background = Shapes.CreateRounded(Palette.Selected, RowRadius);
        Shapes.InsertBehindContent(row.Visual, background);
        row.AddChild(CreateGlobalText());
        row.AddChild(open);
        return row;
    }

    private StackPanel CreateLobbyLoadFailure()
    {
        StackPanel failure = CreateLoadFailure(HandleRetryLobbyClick);
        failure.BindingContext = _tab.Lobby;
        failure.SetBinding(nameof(IsVisible), nameof(LobbyChatViewModel.HasLoadFailed));
        return failure;
    }

    private StackPanel CreateConversationsLoadFailure()
    {
        StackPanel failure = CreateLoadFailure(HandleRetryConversationsClick);
        failure.SetBinding(nameof(IsVisible), nameof(ChatTabViewModel.HasConversationsLoadFailed));
        return failure;
    }

    private StackPanel CreateLoadFailure(EventHandler handleRetryClick)
    {
        Button retry = ViewFactory.CreateButton(nameof(Strings.ChatRetry), RetryButtonWidth, RetryButtonHeight);
        ButtonSkin.CreateAndAttach(retry, ButtonLooks.Secondary);
        retry.Click += handleRetryClick;

        var failure = new StackPanel { Spacing = ListSpacing };
        failure.AddChild(ViewFactory.CreateLabel(nameof(Strings.ChatLoadFailed), TextStyles.FieldError));
        failure.AddChild(retry);
        return failure;
    }

    private void HandleConversationsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        while (_conversations.InnerPanel.Children.Count > 0)
        {
            _conversations.InnerPanel.Children.RemoveAt(_conversations.InnerPanel.Children.Count - 1);
        }

        foreach (ConversationRowViewModel row in _tab.Conversations)
        {
            _conversations.InnerPanel.AddChild(new ConversationRowView(row, _tab).Visual);
        }
    }

    private void HandleOpenGlobalClick(object? sender, EventArgs e)
    {
        _tab.OpenGlobal();
    }

    private async void HandleRetryLobbyClick(object? sender, EventArgs e)
    {
        try
        {
            await _tab.Lobby.LoadChannel();
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("The lobby chat could not be reloaded because the connection is closed.", ex);
        }
    }

    private async void HandleRetryConversationsClick(object? sender, EventArgs e)
    {
        try
        {
            await _tab.LoadConversations();
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("The private conversations could not be reloaded because the connection is closed.", ex);
        }
    }
}
