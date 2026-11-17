using System;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms;
using Gum.Forms.Controls;
using Gum.GueDeriving;
using Gum.Wireframe;
using log4net;

namespace Bastion.Client.Screens.MainMenu.Chat;

public sealed class ChatChannelListView : StackPanel
{
    private const float ListWidth = PanelMetrics.ContentWidth;
    private const float ListSpacing = 12;
    private const float RowHeight = 64;
    private const float RowPaddingX = 14;
    private const float OpenButtonWidth = 88;
    private const float OpenButtonHeight = 36;
    private const float ConversationsHeight = 370;
    private const float ConversationSpacing = 8;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(ChatChannelListView));

    private readonly ChatTabViewModel _chatTab;

    public ChatChannelListView(ChatTabViewModel chatTab)
    {
        ArgumentNullException.ThrowIfNull(chatTab);

        _chatTab = chatTab;
        Width = ListWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Spacing = ListSpacing;
        Panel globalRow = CreateGlobalRow();
        StackPanel lobbyLoadFailure = CreateLobbyLoadFailure();
        Label friendsTitle = ViewFactory.CreateLabel(
            nameof(Strings.ChatWithFriendsTitle),
            TextStyleCatalog.SectionLabel);
        StackPanel noConversations = CreateNoConversations();
        StackPanel conversationsLoadFailure = CreateConversationsLoadFailure();
        ItemsControl conversations = CreateConversations();
        AddChild(globalRow);
        AddChild(lobbyLoadFailure);
        AddChild(friendsTitle);
        AddChild(noConversations);
        AddChild(conversationsLoadFailure);
        AddChild(conversations);
    }

    private Panel CreateGlobalRow()
    {
        Button open = ViewFactory.CreateButton(nameof(Strings.ChatOpen), OpenButtonWidth, OpenButtonHeight);
        ButtonSkin.CreateAndAttach(open, ButtonLookCatalog.Secondary);
        open.Anchor(Gum.Wireframe.Anchor.Right);
        open.X = -RowPaddingX;
        open.Click += HandleOpenGlobalClick;

        var row = new Panel
        {
            Width = ListWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Height = RowHeight,
            HeightUnits = DimensionUnitType.Absolute,
            BindingContext = _chatTab.LobbyChat,
        };
        RectangleRuntime background = ShapeFactory.CreateRounded(Palette.Selected, PanelMetrics.RowRadius);
        ShapeFactory.InsertBehindContent(row.Visual, background);
        StackPanel globalText = CreateGlobalText();
        row.AddChild(globalText);
        row.AddChild(open);
        return row;
    }

    private void HandleOpenGlobalClick(object? sender, EventArgs e)
    {
        _chatTab.OpenGlobal();
    }

    private static StackPanel CreateGlobalText()
    {
        Label detail = ConversationPartFactory.CreateGlobalChannelDetail();

        var text = new StackPanel
        {
            Spacing = PanelMetrics.TextSpacing,
            X = RowPaddingX,
        };
        Label title = ConversationPartFactory.CreateGlobalChannelTitle();
        text.AddChild(title);
        text.AddChild(detail);
        ViewFactory.CenterVertically(text);
        return text;
    }

    private StackPanel CreateLobbyLoadFailure()
    {
        StackPanel failure = ViewFactory.CreateLoadFailure(
            nameof(Strings.ChatLoadFailed),
            nameof(Strings.ChatRetry),
            HandleRetryLobbyClick);
        failure.BindingContext = _chatTab.LobbyChat;
        failure.SetBinding(nameof(IsVisible), nameof(LobbyChatViewModel.HasLoadFailed));
        return failure;
    }

    private async void HandleRetryLobbyClick(object? sender, EventArgs e)
    {
        try
        {
            await _chatTab.LobbyChat.LoadChannel();
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("The lobby chat could not be reloaded because the connection is closed.", ex);
        }
    }

    private static StackPanel CreateNoConversations()
    {
        var empty = new StackPanel
        {
            Spacing = PanelMetrics.TextSpacing,
        };
        Label title = ViewFactory.CreateLabel(nameof(Strings.ChatNoConversationsTitle), TextStyleCatalog.NoticeTitle);
        empty.AddChild(title);
        Label hint = ConversationPartFactory.CreateWrappedLabel(
            nameof(Strings.ChatNoConversationsHint),
            TextStyleCatalog.Hint);
        empty.AddChild(hint);
        empty.SetBinding(nameof(IsVisible), nameof(ChatTabViewModel.HasNoConversations));
        return empty;
    }

    private StackPanel CreateConversationsLoadFailure()
    {
        StackPanel failure = ViewFactory.CreateLoadFailure(
            nameof(Strings.ChatLoadFailed),
            nameof(Strings.ChatRetry),
            HandleRetryConversationsClick);
        failure.SetBinding(nameof(IsVisible), nameof(ChatTabViewModel.HasConversationsLoadFailed));
        return failure;
    }

    private async void HandleRetryConversationsClick(object? sender, EventArgs e)
    {
        try
        {
            await _chatTab.LoadConversations();
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("The private conversations could not be reloaded because the connection is closed.", ex);
        }
    }

    private ItemsControl CreateConversations()
    {
        ItemsControl conversations = ViewFactory.CreateScrollList<ItemsControl>(ListWidth, ConversationsHeight);
        conversations.InnerPanel.StackSpacing = ConversationSpacing;
        conversations.VisualTemplate = new VisualTemplate(CreateConversationRow);
        conversations.Items = _chatTab.Conversations;
        conversations.SetBinding(nameof(IsVisible), nameof(ChatTabViewModel.HasConversations));
        return conversations;
    }

    private GraphicalUiElement CreateConversationRow(object? item)
    {
        ConversationRowViewModel row = ViewFactory.RequireRow<ConversationRowViewModel>(item);
        var rowView = new ConversationRowView(row, _chatTab);
        return rowView.Visual;
    }
}
