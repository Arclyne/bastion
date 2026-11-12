using System;
using Bastion.Client.Controls;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;

namespace Bastion.Client.Screens.MainMenu.Chat;

public static class ConversationParts
{
    public const float ViewWidth = PanelMetrics.ContentWidth;

    private const float HeaderSpacing = 12;
    private const float TextSpacing = 2;
    private const float BackButtonWidth = 96;
    private const float BackButtonHeight = 36;
    private const float InputHeight = 44;
    private const float SendButtonWidth = 96;
    private const float InputSpacing = 8;

    public static StackPanel CreateHeader(EventHandler handleBackClick, Label title, Label detail)
    {
        ArgumentNullException.ThrowIfNull(handleBackClick);
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(detail);

        Button back = ViewFactory.CreateButton(nameof(Strings.ChatBack), BackButtonWidth, BackButtonHeight);
        ButtonSkin.CreateAndAttach(back, ButtonLookCatalog.Pill);
        back.Click += handleBackClick;

        var titleLines = new StackPanel
        {
            Spacing = TextSpacing,
        };
        titleLines.AddChild(title);
        titleLines.AddChild(detail);

        var header = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = HeaderSpacing,
        };
        header.AddChild(back);
        header.AddChild(titleLines);
        return header;
    }

    public static Label CreateWrappedLabel(string textKey, TextStyle style)
    {
        ArgumentNullException.ThrowIfNull(textKey);
        ArgumentNullException.ThrowIfNull(style);

        Label label = ViewFactory.CreateLabel(textKey, style);
        label.Width = ViewWidth;
        label.WidthUnits = DimensionUnitType.Absolute;
        return label;
    }

    public static Label CreateNoticeLabel(string textProperty, string visibilityProperty, TextStyle style)
    {
        ArgumentNullException.ThrowIfNull(textProperty);
        ArgumentNullException.ThrowIfNull(visibilityProperty);
        ArgumentNullException.ThrowIfNull(style);

        Label label = ViewFactory.CreateBoundNotice(textProperty, visibilityProperty, style);
        label.Width = ViewWidth;
        label.WidthUnits = DimensionUnitType.Absolute;
        return label;
    }

    public static ItemsControl CreateMessageList(float height, float rowSpacing)
    {
        ItemsControl messages = ViewFactory.CreateScrollList<ItemsControl>(ViewWidth, height);
        messages.InnerPanel.StackSpacing = rowSpacing;
        return messages;
    }

    public static StackPanel CreateInputRow(EventHandler handleSendClick, float rowWidth)
    {
        ArgumentNullException.ThrowIfNull(handleSendClick);

        var input = new TextField
        {
            Placeholder = nameof(Strings.ChatPlaceholder),
            Width = rowWidth - SendButtonWidth - InputSpacing,
            Height = InputHeight,
        };
        input.SetBinding(nameof(TextField.Text), nameof(ChatComposerViewModel.Draft));
        input.SetBinding(nameof(FrameworkElement.IsEnabled), nameof(ChatComposerViewModel.CanWrite));

        Button send = ViewFactory.CreateButton(nameof(Strings.ChatSend), SendButtonWidth, InputHeight);
        ButtonSkin.CreateAndAttach(send, ButtonLookCatalog.Primary);
        send.SetBinding(nameof(FrameworkElement.IsEnabled), nameof(ChatComposerViewModel.CanSend));
        send.Click += handleSendClick;

        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = InputSpacing,
        };
        row.AddChild(input);
        row.AddChild(send);
        return row;
    }

    public static void ScrollToLatest(ScrollViewer list)
    {
        ArgumentNullException.ThrowIfNull(list);

        list.VerticalScrollBarValue = list.VerticalScrollBarMaximum;
    }
}
