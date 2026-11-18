using System;
using Bastion.Client.Controls;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.Forms.Controls;

namespace Bastion.Client.Screens.MainMenu.Chat;

public static class ConversationPartFactory
{
    public const float ViewWidth = PanelMetrics.ContentWidth;
    public const float MessageRowWidth = ViewWidth - (MessageRowMarginX * HorizontalSideCount);

    private const float HeaderSpacing = 12;
    private const float BackButtonWidth = 96;
    private const float BackButtonHeight = 36;
    private const float InputHeight = 44;
    private const float SendButtonWidth = 96;
    private const float InputSpacing = 8;
    private const float MessageRowMarginX = 10;
    private const int HorizontalSideCount = 2;

    public static StackPanel CreateHeader(EventHandler handleBackClick, Label title, Label detail)
    {
        ArgumentNullException.ThrowIfNull(handleBackClick);
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(detail);

        Button backButton = ViewFactory.CreateButton(nameof(Strings.ChatBack), BackButtonWidth, BackButtonHeight);
        ButtonSkin.CreateAndAttach(backButton, ButtonLookCatalog.Pill);
        backButton.Click += handleBackClick;

        var titleLines = new StackPanel
        {
            Spacing = PanelMetrics.TextSpacing,
        };
        titleLines.AddChild(title);
        titleLines.AddChild(detail);

        var header = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = HeaderSpacing,
        };
        header.AddChild(backButton);
        header.AddChild(titleLines);
        return header;
    }

    public static Label CreateWrappedLabel(string textKey, TextStyle style)
    {
        ArgumentNullException.ThrowIfNull(textKey);
        ArgumentNullException.ThrowIfNull(style);

        Label label = ViewFactory.CreateLabel(textKey, style);
        ViewFactory.SetFixedWidth(label, ViewWidth);
        return label;
    }

    public static Label CreateNoticeLabel(string textProperty, string visibilityProperty, TextStyle style)
    {
        ArgumentNullException.ThrowIfNull(textProperty);
        ArgumentNullException.ThrowIfNull(visibilityProperty);
        ArgumentNullException.ThrowIfNull(style);

        Label label = ViewFactory.CreateBoundNotice(textProperty, visibilityProperty, style);
        ViewFactory.SetFixedWidth(label, ViewWidth);
        return label;
    }

    public static ItemsControl CreateMessageList(float height, float rowSpacing)
    {
        ItemsControl messages = ViewFactory.CreateScrollList<ItemsControl>(ViewWidth, height);
        messages.InnerPanel.StackSpacing = rowSpacing;
        return messages;
    }

    public static Label CreateGlobalChannelTitle()
    {
        return ViewFactory.CreateLabel(nameof(Strings.ChatGlobalChannel), TextStyleCatalog.IdentityName);
    }

    public static Label CreateGlobalChannelDetail()
    {
        return ViewFactory.CreateBoundUntranslatedLabel(
            nameof(LobbyChatViewModel.ChannelDetail),
            TextStyleCatalog.Hint);
    }

    public static StackPanel CreateLoadFailure(EventHandler handleRetryClick, string visibilityProperty)
    {
        ArgumentNullException.ThrowIfNull(handleRetryClick);
        ArgumentNullException.ThrowIfNull(visibilityProperty);

        StackPanel failure = ViewFactory.CreateLoadFailure(
            nameof(Strings.ChatLoadFailed),
            nameof(Strings.ChatRetry),
            handleRetryClick);
        failure.SetBinding(nameof(StackPanel.IsVisible), visibilityProperty);
        return failure;
    }

    public static StackPanel CreateComposer(EventHandler handleSendClick)
    {
        ArgumentNullException.ThrowIfNull(handleSendClick);

        Label statusNotice = CreateNoticeLabel(
            nameof(ChatComposerViewModel.StatusText),
            nameof(ChatComposerViewModel.HasStatus),
            TextStyleCatalog.FieldError);
        Label counterNotice = CreateNoticeLabel(
            nameof(ChatComposerViewModel.CounterText),
            nameof(ChatComposerViewModel.IsCounterVisible),
            TextStyleCatalog.Hint);
        StackPanel inputRow = CreateInputRow(handleSendClick, ViewWidth);
        var composer = new StackPanel
        {
            Spacing = PanelMetrics.SectionSpacing,
        };
        composer.AddChild(statusNotice);
        composer.AddChild(counterNotice);
        composer.AddChild(inputRow);
        return composer;
    }

    public static StackPanel CreateInputRow(EventHandler handleSendClick, float rowWidth)
    {
        ArgumentNullException.ThrowIfNull(handleSendClick);

        float inputWidth = rowWidth - SendButtonWidth - InputSpacing;
        var input = new TextField
        {
            Placeholder = nameof(Strings.ChatPlaceholder),
            Width = inputWidth,
            Height = InputHeight,
        };
        input.SetBinding(nameof(TextField.Text), nameof(ChatComposerViewModel.Draft));
        input.SetBinding(nameof(FrameworkElement.IsEnabled), nameof(ChatComposerViewModel.CanWrite));

        Button sendButton = ViewFactory.CreateButton(nameof(Strings.ChatSend), SendButtonWidth, InputHeight);
        ButtonSkin.CreateAndAttach(sendButton, ButtonLookCatalog.Primary);
        sendButton.SetBinding(nameof(FrameworkElement.IsEnabled), nameof(ChatComposerViewModel.CanSend));
        sendButton.Click += handleSendClick;

        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = InputSpacing,
        };
        row.AddChild(input);
        row.AddChild(sendButton);
        return row;
    }

    public static void ScrollToLatest(ScrollViewer list)
    {
        ArgumentNullException.ThrowIfNull(list);

        list.VerticalScrollBarValue = list.VerticalScrollBarMaximum;
    }
}
