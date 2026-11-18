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

    public static StackPanel CreateHeader(EventHandler handleBackClick, Label titleLabel, Label detailLabel)
    {
        ArgumentNullException.ThrowIfNull(handleBackClick);
        ArgumentNullException.ThrowIfNull(titleLabel);
        ArgumentNullException.ThrowIfNull(detailLabel);

        Button backButton = ViewFactory.CreateButton(nameof(Strings.ChatBack), BackButtonWidth, BackButtonHeight);
        ButtonSkin.CreateAndAttach(backButton, ButtonLookCatalog.Pill);
        backButton.Click += handleBackClick;

        var titleLinesStackPanel = new StackPanel
        {
            Spacing = PanelMetrics.TextSpacing,
        };
        titleLinesStackPanel.AddChild(titleLabel);
        titleLinesStackPanel.AddChild(detailLabel);

        var headerStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = HeaderSpacing,
        };
        headerStackPanel.AddChild(backButton);
        headerStackPanel.AddChild(titleLinesStackPanel);
        return headerStackPanel;
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
        ItemsControl messagesItemsControl = ViewFactory.CreateScrollList<ItemsControl>(ViewWidth, height);
        messagesItemsControl.InnerPanel.StackSpacing = rowSpacing;
        return messagesItemsControl;
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

        StackPanel failureStackPanel = ViewFactory.CreateLoadFailure(
            nameof(Strings.ChatLoadFailed),
            nameof(Strings.ChatRetry),
            handleRetryClick);
        failureStackPanel.SetBinding(nameof(StackPanel.IsVisible), visibilityProperty);
        return failureStackPanel;
    }

    public static StackPanel CreateComposer(EventHandler handleSendClick)
    {
        ArgumentNullException.ThrowIfNull(handleSendClick);

        Label statusNoticeLabel = CreateNoticeLabel(
            nameof(ChatComposerViewModel.StatusText),
            nameof(ChatComposerViewModel.HasStatus),
            TextStyleCatalog.FieldError);
        Label counterNoticeLabel = CreateNoticeLabel(
            nameof(ChatComposerViewModel.CounterText),
            nameof(ChatComposerViewModel.IsCounterVisible),
            TextStyleCatalog.Hint);
        StackPanel inputRowStackPanel = CreateInputRow(handleSendClick, ViewWidth);
        var composerStackPanel = new StackPanel
        {
            Spacing = PanelMetrics.SectionSpacing,
        };
        composerStackPanel.AddChild(statusNoticeLabel);
        composerStackPanel.AddChild(counterNoticeLabel);
        composerStackPanel.AddChild(inputRowStackPanel);
        return composerStackPanel;
    }

    public static StackPanel CreateInputRow(EventHandler handleSendClick, float rowWidth)
    {
        ArgumentNullException.ThrowIfNull(handleSendClick);

        float inputWidth = rowWidth - SendButtonWidth - InputSpacing;
        var inputTextField = new TextField
        {
            Placeholder = nameof(Strings.ChatPlaceholder),
            Width = inputWidth,
            Height = InputHeight,
        };
        inputTextField.SetBinding(nameof(TextField.Text), nameof(ChatComposerViewModel.Draft));
        inputTextField.SetBinding(nameof(FrameworkElement.IsEnabled), nameof(ChatComposerViewModel.CanWrite));

        Button sendButton = ViewFactory.CreateButton(nameof(Strings.ChatSend), SendButtonWidth, InputHeight);
        ButtonSkin.CreateAndAttach(sendButton, ButtonLookCatalog.Primary);
        sendButton.SetBinding(nameof(FrameworkElement.IsEnabled), nameof(ChatComposerViewModel.CanSend));
        sendButton.Click += handleSendClick;

        var rowStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = InputSpacing,
        };
        rowStackPanel.AddChild(inputTextField);
        rowStackPanel.AddChild(sendButton);
        return rowStackPanel;
    }

    public static void ScrollToLatest(ScrollViewer listScrollViewer)
    {
        ArgumentNullException.ThrowIfNull(listScrollViewer);

        listScrollViewer.VerticalScrollBarValue = listScrollViewer.VerticalScrollBarMaximum;
    }
}
