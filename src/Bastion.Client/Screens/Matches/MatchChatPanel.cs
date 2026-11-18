using System;
using System.Collections.Specialized;
using Bastion.Client.Localization;
using Bastion.Client.Screens.MainMenu.Chat;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using log4net;

namespace Bastion.Client.Screens.Matches;

public sealed class MatchChatPanel : Panel
{
    private const float PanelWidth = 320;
    private const float PanelHeight = 520;
    private const float PaddingX = 18;
    private const float PaddingTop = 18;
    private const float ContentWidth = PanelWidth - (PaddingX * HorizontalSideCount);
    private const float ContentSpacing = 10;
    private const float MessagesHeight = 360;
    private const int HorizontalSideCount = 2;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(MatchChatPanel));

    private readonly MatchChatViewModel _viewModel;
    private readonly ItemsControl _messageScrollerItemsControl;

    public MatchChatPanel(MatchChatViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _viewModel = viewModel;
        BindingContext = viewModel;
        Width = PanelWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Height = PanelHeight;
        HeightUnits = DimensionUnitType.Absolute;

        _messageScrollerItemsControl = CreateMessages();

        var contentStackPanel = new StackPanel
        {
            X = PaddingX,
            Y = PaddingTop,
            Width = ContentWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Spacing = ContentSpacing,
        };
        Label titleLabel = ViewFactory.CreateLabel(nameof(Strings.MatchChatTitle), TextStyleCatalog.SectionLabel);
        Label statusLabel = CreateStatus();
        StackPanel inputRowStackPanel = ConversationPartFactory.CreateInputRow(HandleSendClick, ContentWidth);
        contentStackPanel.AddChild(titleLabel);
        contentStackPanel.AddChild(_messageScrollerItemsControl);
        contentStackPanel.AddChild(statusLabel);
        contentStackPanel.AddChild(inputRowStackPanel);
        AddChild(contentStackPanel);
        CardSkin.Apply(Visual);

        _viewModel.Messages.CollectionChanged += HandleMessagesChanged;
    }

    private ItemsControl CreateMessages()
    {
        ItemsControl messagesItemsControl = ViewFactory.CreateScrollList<ItemsControl>(ContentWidth, MessagesHeight);
        messagesItemsControl.VisualTemplate = ChatMessageRowView.CreateTemplate(ContentWidth);
        messagesItemsControl.Items = _viewModel.Messages;
        return messagesItemsControl;
    }

    private static Label CreateStatus()
    {
        Label statusLabel = ViewFactory.CreateBoundNotice(
            nameof(MatchChatViewModel.StatusText),
            nameof(MatchChatViewModel.HasStatus),
            TextStyleCatalog.FieldError);
        ViewFactory.SetFixedWidth(statusLabel, ContentWidth);
        return statusLabel;
    }

    private async void HandleSendClick(object? sender, EventArgs e)
    {
        try
        {
            await _viewModel.SendMessage();
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("The match chat message could not be sent because the connection is closed.", ex);
        }
    }

    private void HandleMessagesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ConversationPartFactory.ScrollToLatest(_messageScrollerItemsControl);
    }
}
