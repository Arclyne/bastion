using System;
using System.Collections;
using System.Collections.Specialized;
using Bastion.Client.Controls;
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
    private const float ContentWidth = 284;
    private const float ContentSpacing = 10;
    private const float MessagesHeight = 360;
    private const float InputHeight = 44;
    private const float SendButtonWidth = 80;
    private const float InputSpacing = 8;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(MatchChatPanel));

    private readonly MatchChatViewModel _viewModel;
    private readonly ScrollViewer _messageScroller;

    public MatchChatPanel(MatchChatViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _viewModel = viewModel;
        BindingContext = viewModel;
        Width = PanelWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Height = PanelHeight;
        HeightUnits = DimensionUnitType.Absolute;

        _messageScroller = new ScrollViewer
        {
            Width = ContentWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Height = MessagesHeight,
            HeightUnits = DimensionUnitType.Absolute,
        };
        Shapes.HideDefaultParts(_messageScroller);

        var content = new StackPanel
        {
            X = PaddingX,
            Y = PaddingTop,
            Width = ContentWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Spacing = ContentSpacing,
        };
        content.AddChild(ViewFactory.CreateLabel(nameof(Strings.MatchChatTitle), TextStyles.SectionLabel));
        content.AddChild(_messageScroller);
        content.AddChild(CreateStatus());
        content.AddChild(CreateInputRow());
        AddChild(content);
        Skins.ApplyCard(Visual);

        AddMessageRows(_viewModel.Messages);
        _viewModel.Messages.CollectionChanged += HandleMessagesCollectionChanged;
    }

    private static Label CreateStatus()
    {
        Label status = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(MatchChatViewModel.StatusText),
            TextStyles.FieldError,
            ContentWidth);
        status.SetBinding(nameof(IsVisible), nameof(MatchChatViewModel.HasStatus));
        return status;
    }

    private StackPanel CreateInputRow()
    {
        var input = new TextField
        {
            Placeholder = nameof(Strings.ChatPlaceholder),
            Width = ContentWidth - SendButtonWidth - InputSpacing,
            Height = InputHeight,
        };
        input.SetBinding(nameof(TextField.Text), nameof(MatchChatViewModel.Draft));

        Button send = ViewFactory.CreateButton(nameof(Strings.ChatSend), SendButtonWidth, InputHeight);
        ButtonSkin.CreateAndAttach(send, ButtonLooks.Secondary);
        send.SetBinding(nameof(IsEnabled), nameof(MatchChatViewModel.CanSend));
        send.Click += HandleSendClick;

        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = InputSpacing,
        };
        row.AddChild(input);
        row.AddChild(send);
        return row;
    }

    private void AddMessageRows(IEnumerable rows)
    {
        foreach (ChatMessageRowViewModel row in rows)
        {
            _messageScroller.InnerPanel.AddChild(new ChatMessageRowView(row).Visual);
        }

        _messageScroller.VerticalScrollBarValue = _messageScroller.VerticalScrollBarMaximum;
    }

    private void HandleMessagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            _messageScroller.InnerPanel.Children.RemoveAt(e.OldStartingIndex);
            return;
        }

        if (e.Action != NotifyCollectionChangedAction.Add || e.NewItems is null)
        {
            return;
        }

        AddMessageRows(e.NewItems);
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
}
