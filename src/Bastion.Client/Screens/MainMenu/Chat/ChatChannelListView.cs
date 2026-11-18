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
        Panel globalRowPanel = CreateGlobalRow();
        StackPanel lobbyLoadFailureStackPanel = CreateLobbyLoadFailure();
        Label friendsTitleLabel = ViewFactory.CreateLabel(
            nameof(Strings.ChatWithFriendsTitle),
            TextStyleCatalog.SectionLabel);
        StackPanel noConversationsStackPanel = CreateNoConversations();
        StackPanel conversationsLoadFailureStackPanel = CreateConversationsLoadFailure();
        ItemsControl conversationsItemsControl = CreateConversations();
        AddChild(globalRowPanel);
        AddChild(lobbyLoadFailureStackPanel);
        AddChild(friendsTitleLabel);
        AddChild(noConversationsStackPanel);
        AddChild(conversationsLoadFailureStackPanel);
        AddChild(conversationsItemsControl);
    }

    private Panel CreateGlobalRow()
    {
        Button openButton = ViewFactory.CreateButton(nameof(Strings.ChatOpen), OpenButtonWidth, OpenButtonHeight);
        ButtonSkin.CreateAndAttach(openButton, ButtonLookCatalog.Secondary);
        openButton.Anchor(Gum.Wireframe.Anchor.Right);
        openButton.X = -RowPaddingX;
        openButton.Click += HandleOpenGlobalClick;

        var rowPanel = new Panel
        {
            Width = ListWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Height = RowHeight,
            HeightUnits = DimensionUnitType.Absolute,
            BindingContext = _chatTab.LobbyChat,
        };
        RectangleRuntime background = ShapeFactory.CreateRounded(Palette.Selected, PanelMetrics.RowRadius);
        ShapeFactory.InsertBehindContent(rowPanel.Visual, background);
        StackPanel globalTextStackPanel = CreateGlobalText();
        rowPanel.AddChild(globalTextStackPanel);
        rowPanel.AddChild(openButton);
        return rowPanel;
    }

    private void HandleOpenGlobalClick(object? sender, EventArgs e)
    {
        _chatTab.OpenGlobal();
    }

    private static StackPanel CreateGlobalText()
    {
        Label titleLabel = ConversationPartFactory.CreateGlobalChannelTitle();
        Label detailLabel = ConversationPartFactory.CreateGlobalChannelDetail();
        StackPanel textStackPanel = ViewFactory.CreateCenteredLines(titleLabel, detailLabel);
        textStackPanel.X = RowPaddingX;
        return textStackPanel;
    }

    private StackPanel CreateLobbyLoadFailure()
    {
        StackPanel failureStackPanel = ConversationPartFactory.CreateLoadFailure(
            HandleRetryLobbyClick,
            nameof(LobbyChatViewModel.HasLoadFailed));
        failureStackPanel.BindingContext = _chatTab.LobbyChat;
        return failureStackPanel;
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
        var emptyStackPanel = new StackPanel
        {
            Spacing = PanelMetrics.TextSpacing,
        };
        Label titleLabel = ViewFactory.CreateLabel(
            nameof(Strings.ChatNoConversationsTitle),
            TextStyleCatalog.NoticeTitle);
        emptyStackPanel.AddChild(titleLabel);
        Label hintLabel = ConversationPartFactory.CreateWrappedLabel(
            nameof(Strings.ChatNoConversationsHint),
            TextStyleCatalog.Hint);
        emptyStackPanel.AddChild(hintLabel);
        emptyStackPanel.SetBinding(nameof(IsVisible), nameof(ChatTabViewModel.HasNoConversations));
        return emptyStackPanel;
    }

    private StackPanel CreateConversationsLoadFailure()
    {
        StackPanel failureStackPanel = ConversationPartFactory.CreateLoadFailure(
            HandleRetryConversationsClick,
            nameof(ChatTabViewModel.HasConversationsLoadFailed));
        return failureStackPanel;
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
        ItemsControl conversationsItemsControl = ViewFactory.CreateScrollList<ItemsControl>(
            ListWidth,
            ConversationsHeight);
        conversationsItemsControl.InnerPanel.StackSpacing = ConversationSpacing;
        conversationsItemsControl.VisualTemplate = new VisualTemplate(CreateConversationRow);
        conversationsItemsControl.Items = _chatTab.Conversations;
        conversationsItemsControl.SetBinding(nameof(IsVisible), nameof(ChatTabViewModel.HasConversations));
        return conversationsItemsControl;
    }

    private GraphicalUiElement CreateConversationRow(object? item)
    {
        ConversationRowViewModel row = ViewFactory.RequireRow<ConversationRowViewModel>(item);
        var conversationRowView = new ConversationRowView(row, _chatTab);
        return conversationRowView.Visual;
    }
}
