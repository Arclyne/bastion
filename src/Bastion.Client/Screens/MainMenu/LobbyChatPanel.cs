using System;
using System.Collections.Specialized;
using Bastion.Client.Controls;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;

namespace Bastion.Client.Screens.MainMenu;

public sealed class LobbyChatPanel : Panel
{
    private const float PanelWidth = 300;
    private const float PanelHeight = 380;
    private const float PaddingX = 20;
    private const float PaddingTop = 24;
    private const float ContentWidth = 260;
    private const float ContentSpacing = 10;
    private const float LinesHeight = 230;
    private const float LineWidth = 236;
    private const float InputHeight = 44;
    private const float SendButtonWidth = 80;
    private const float InputSpacing = 8;

    private readonly MainMenuViewModel _viewModel;
    private readonly ScrollViewer _lines;

    public LobbyChatPanel(MainMenuViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _viewModel = viewModel;
        Width = PanelWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Height = PanelHeight;
        HeightUnits = DimensionUnitType.Absolute;

        _lines = new ScrollViewer
        {
            Width = ContentWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Height = LinesHeight,
            HeightUnits = DimensionUnitType.Absolute,
        };

        var content = new StackPanel
        {
            X = PaddingX,
            Y = PaddingTop,
            Width = ContentWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Spacing = ContentSpacing,
        };
        content.AddChild(ViewFactory.CreateLabel(nameof(Strings.MainMenuChatTitle), TextStyles.SectionLabel));
        content.AddChild(_lines);
        content.AddChild(CreateStatus());
        content.AddChild(CreateInputRow());
        AddChild(content);
        Skins.ApplyCard(Visual);

        _viewModel.ChatLines.CollectionChanged += OnChatLinesChanged;
    }

    private static Label CreateStatus()
    {
        Label status = ViewFactory.CreateLabel(string.Empty, TextStyles.FieldError);
        status.SetBinding(nameof(Label.Text), nameof(MainMenuViewModel.ChatStatusKey));
        status.SetBinding(nameof(IsVisible), nameof(MainMenuViewModel.HasChatStatus));
        return status;
    }

    private StackPanel CreateInputRow()
    {
        var input = new TextField
        {
            Placeholder = nameof(Strings.MainMenuChatPlaceholder),
            Width = ContentWidth - SendButtonWidth - InputSpacing,
            Height = InputHeight,
        };
        input.SetBinding(nameof(TextField.Text), nameof(MainMenuViewModel.ChatDraft));

        Button send = ViewFactory.CreateButton(nameof(Strings.MainMenuChatSend), SendButtonWidth, InputHeight);
        ButtonSkin.Apply(send, ButtonLooks.Secondary);
        send.SetBinding(nameof(IsEnabled), nameof(MainMenuViewModel.CanSend));
        send.Click += OnSendClick;

        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = InputSpacing };
        row.AddChild(input);
        row.AddChild(send);
        return row;
    }

    private void OnChatLinesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            _lines.InnerPanel.Children.RemoveAt(e.OldStartingIndex);
            return;
        }

        if (e.Action != NotifyCollectionChangedAction.Add || e.NewItems is null)
        {
            return;
        }

        foreach (string line in e.NewItems)
        {
            _lines.InnerPanel.AddChild(CreateLine(line).Visual);
        }

        _lines.VerticalScrollBarValue = _lines.VerticalScrollBarMaximum;
    }

    private static Label CreateLine(string text)
    {
        Label label = ViewFactory.CreateLabel(string.Empty, TextStyles.NoticeMessage);
        ((LabelVisual)label.Visual).LocalizeText = false;
        label.Text = text;
        label.Width = LineWidth;
        label.WidthUnits = DimensionUnitType.Absolute;
        return label;
    }

    private async void OnSendClick(object? sender, EventArgs e)
    {
        await _viewModel.SendChatMessage();
    }
}
