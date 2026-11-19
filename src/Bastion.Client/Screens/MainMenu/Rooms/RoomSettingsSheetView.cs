using System;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.GueDeriving;
using log4net;

namespace Bastion.Client.Screens.MainMenu.Rooms;

public sealed class RoomSettingsSheetView : DialogCard
{
    private const float OptionSpacing = 10;
    private const int OptionsPerRow = 3;
    private const float OptionGaps = OptionSpacing * (OptionsPerRow - 1);
    private const float OptionWidth = (DialogMetrics.ContentWidth - OptionGaps) / OptionsPerRow;
    private const float ModeHeight = 78;
    private const float ModePaddingX = 12;
    private const float ModePaddingTop = 10;
    private const float ModeTextSpacing = 3;
    private const float OptionRadius = 14;
    private const float SelectedBorderWidth = 2.5f;
    private const float ClockHeight = 42;
    private const float StepperSize = 42;
    private const float StepperValueWidth = 64;
    private const float StepperSpacing = 8;
    private const float StepperRadius = 12;
    private const float StepperBorderWidth = 1.8f;
    private const float WallsRowSpacing = 14;
    private const float WallsHintWidth =
        DialogMetrics.ContentWidth - (StepperSize * 2) - StepperValueWidth - (StepperSpacing * 2) - WallsRowSpacing;
    private const float NoticePadding = 10;
    private const float NoticeRadius = 12;
    private const int SideCount = 2;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(RoomSettingsSheetView));

    private readonly PrivateRoomViewModel _room;
    private readonly RoomSettingsSheetViewModel _sheet;

    public RoomSettingsSheetView(PrivateRoomViewModel room)
    {
        ArgumentNullException.ThrowIfNull(room);

        _room = room;
        _sheet = room.Sheet;
        BindingContext = room.Sheet;
        Anchor(Gum.Wireframe.Anchor.Center);
        Label titleLabel = ViewFactory.CreateBoundLabel(
            nameof(RoomSettingsSheetViewModel.TitleKey),
            TextStyleCatalog.DialogTitle,
            DialogMetrics.ContentWidth);
        Content.AddChild(titleLabel);
        Content.AddChild(ViewFactory.CreateLabel(nameof(Strings.RoomModeLabel), TextStyleCatalog.FieldLabel));
        Content.AddChild(CreateModes());
        Content.AddChild(ViewFactory.CreateLabel(nameof(Strings.RoomWallsLabel), TextStyleCatalog.FieldLabel));
        Content.AddChild(CreateWalls());
        Content.AddChild(ViewFactory.CreateLabel(nameof(Strings.RoomClockLabel), TextStyleCatalog.FieldLabel));
        Content.AddChild(CreateClocks());
        Content.AddChild(CreateMembersNotice());
        Content.AddChild(ViewFactory.CreateLabel(nameof(Strings.RoomFriendly), TextStyleCatalog.RoomFriendly));
        Content.AddChild(CreateError());
        Content.AddChild(CreateButtons());
    }

    private StackPanel CreateModes()
    {
        var rowStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = OptionSpacing,
        };
        foreach (RoomModeOptionViewModel option in _sheet.Modes)
        {
            rowStackPanel.AddChild(CreateModeCard(option));
        }

        return rowStackPanel;
    }

    private Button CreateModeCard(RoomModeOptionViewModel option)
    {
        Button button = ViewFactory.CreateButton(string.Empty, OptionWidth, ModeHeight);
        ButtonSkin.CreateAndAttach(button, ButtonLookCatalog.Secondary);
        button.BindingContext = option;
        button.SetBinding(nameof(IsEnabled), nameof(RoomModeOptionViewModel.IsAvailable));
        button.Visual.ExposeChildrenEvents = false;
        Panel selectedPanel = CreateSelectedFrame(nameof(RoomModeOptionViewModel.IsSelected));
        var textStackPanel = new StackPanel
        {
            X = ModePaddingX,
            Y = ModePaddingTop,
            Spacing = ModeTextSpacing,
        };
        textStackPanel.AddChild(ViewFactory.CreateUntranslatedLabel(option.NameText, TextStyleCatalog.RoomModeName));
        textStackPanel.AddChild(ViewFactory.CreateUntranslatedLabel(
            option.DetailText,
            TextStyleCatalog.RoomModeDetail));
        textStackPanel.AddChild(ViewFactory.CreateBoundUntranslatedLabel(
            nameof(RoomModeOptionViewModel.CaptionText),
            TextStyleCatalog.RoomModeCaption));
        button.AddChild(selectedPanel);
        button.AddChild(textStackPanel);
        button.Click += (_, _) => _sheet.SelectMode(option.Mode);
        return button;
    }

    private static Panel CreateSelectedFrame(string visibilityProperty)
    {
        var framePanel = new Panel();
        framePanel.Dock(Gum.Wireframe.Dock.Fill);
        framePanel.SetBinding(nameof(IsVisible), visibilityProperty);
        RectangleRuntime frame = ShapeFactory.CreateRounded(Palette.RejoinSoft, OptionRadius);
        frame.StrokeColor = Palette.Accent;
        frame.StrokeWidth = SelectedBorderWidth;
        ShapeFactory.InsertBehindContent(framePanel.Visual, frame);
        return framePanel;
    }

    private StackPanel CreateWalls()
    {
        var rowStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = WallsRowSpacing,
        };
        var stepperStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = StepperSpacing,
        };
        Button decreaseButton = CreateStepperButton(
            nameof(Strings.RoomMinus),
            nameof(RoomSettingsSheetViewModel.CanDecreaseWalls));
        decreaseButton.Click += (_, _) => _sheet.DecreaseWalls();
        Button increaseButton = CreateStepperButton(
            nameof(Strings.RoomPlus),
            nameof(RoomSettingsSheetViewModel.CanIncreaseWalls));
        increaseButton.Click += (_, _) => _sheet.IncreaseWalls();
        stepperStackPanel.AddChild(decreaseButton);
        stepperStackPanel.AddChild(CreateStepperValue());
        stepperStackPanel.AddChild(increaseButton);
        Label hintLabel = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(RoomSettingsSheetViewModel.WallsHintText),
            TextStyleCatalog.Hint,
            WallsHintWidth);
        ViewFactory.CenterVertically(hintLabel);
        rowStackPanel.AddChild(stepperStackPanel);
        rowStackPanel.AddChild(hintLabel);
        return rowStackPanel;
    }

    private static Button CreateStepperButton(string textKey, string enabledProperty)
    {
        Button button = ViewFactory.CreateButton(textKey, StepperSize, StepperSize);
        ButtonSkin.CreateAndAttach(button, ButtonLookCatalog.Secondary);
        button.SetBinding(nameof(IsEnabled), enabledProperty);
        return button;
    }

    private static Panel CreateStepperValue()
    {
        var valuePanel = new Panel
        {
            Width = StepperValueWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Height = StepperSize,
            HeightUnits = DimensionUnitType.Absolute,
        };
        RectangleRuntime background = ShapeFactory.CreateRounded(Palette.Card, StepperRadius);
        background.StrokeColor = Palette.Border;
        background.StrokeWidth = StepperBorderWidth;
        ShapeFactory.InsertBehindContent(valuePanel.Visual, background);
        Label valueLabel = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(RoomSettingsSheetViewModel.WallsText),
            TextStyleCatalog.RoomStepperValue);
        ViewFactory.CenterHorizontally(valueLabel);
        ViewFactory.CenterVertically(valueLabel);
        valuePanel.AddChild(valueLabel);
        return valuePanel;
    }

    private StackPanel CreateClocks()
    {
        var rowStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = OptionSpacing,
        };
        foreach (RoomClockOptionViewModel clock in _sheet.Clocks)
        {
            rowStackPanel.AddChild(CreateClockButton(clock));
        }

        return rowStackPanel;
    }

    private Button CreateClockButton(RoomClockOptionViewModel clock)
    {
        Button button = ViewFactory.CreateButton(string.Empty, OptionWidth, ClockHeight);
        ButtonSkin.CreateAndAttach(button, ButtonLookCatalog.Secondary);
        button.BindingContext = clock;
        button.Visual.ExposeChildrenEvents = false;
        var selectedPanel = new Panel();
        selectedPanel.Dock(Gum.Wireframe.Dock.Fill);
        selectedPanel.SetBinding(nameof(IsVisible), nameof(RoomClockOptionViewModel.IsSelected));
        RectangleRuntime fill = ShapeFactory.CreateRounded(Palette.Accent, StepperRadius);
        ShapeFactory.InsertBehindContent(selectedPanel.Visual, fill);
        selectedPanel.AddChild(CreateCenteredText(clock.Text, TextStyleCatalog.RoomClockSelected));
        Label idleLabel = CreateCenteredText(clock.Text, TextStyleCatalog.RoomClockIdle);
        idleLabel.SetBinding(nameof(IsVisible), nameof(RoomClockOptionViewModel.IsIdle));
        button.AddChild(selectedPanel);
        button.AddChild(idleLabel);
        button.Click += (_, _) => _sheet.SelectClock(clock.Minutes);
        return button;
    }

    private static Label CreateCenteredText(string text, TextStyle style)
    {
        Label label = ViewFactory.CreateUntranslatedLabel(text, style);
        ViewFactory.CenterHorizontally(label);
        ViewFactory.CenterVertically(label);
        return label;
    }

    private static Panel CreateMembersNotice()
    {
        var noticePanel = new Panel
        {
            Width = DialogMetrics.ContentWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Height = NoticePadding * SideCount,
            HeightUnits = DimensionUnitType.RelativeToChildren,
        };
        noticePanel.SetBinding(nameof(IsVisible), nameof(RoomSettingsSheetViewModel.HasMembersNotice));
        RectangleRuntime background = ShapeFactory.CreateRounded(Palette.WarningSoft, NoticeRadius);
        ShapeFactory.InsertBehindContent(noticePanel.Visual, background);
        Label messageLabel = ViewFactory.CreateLabel(nameof(Strings.RoomMembersNotice), TextStyleCatalog.NoticeMessage);
        ViewFactory.SetFixedWidth(messageLabel, DialogMetrics.ContentWidth - (NoticePadding * SideCount));
        messageLabel.X = NoticePadding;
        messageLabel.Y = NoticePadding;
        noticePanel.AddChild(messageLabel);
        return noticePanel;
    }

    private static Label CreateError()
    {
        Label errorLabel = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(RoomSettingsSheetViewModel.ErrorText),
            TextStyleCatalog.FieldError,
            DialogMetrics.ContentWidth);
        errorLabel.SetBinding(nameof(IsVisible), nameof(RoomSettingsSheetViewModel.HasError));
        return errorLabel;
    }

    private StackPanel CreateButtons()
    {
        Button cancelButton = CreateDialogButton(nameof(Strings.RoomCancel), ButtonLookCatalog.Secondary);
        cancelButton.Click += HandleCancelClick;
        Button confirmButton = ViewFactory.CreateBoundButton(
            nameof(RoomSettingsSheetViewModel.ConfirmKey),
            DialogMetrics.ButtonWidth,
            DialogMetrics.ButtonHeight);
        ButtonSkin.CreateAndAttach(confirmButton, ButtonLookCatalog.Primary);
        confirmButton.SetBinding(nameof(IsEnabled), nameof(RoomSettingsSheetViewModel.CanConfirm));
        confirmButton.Click += HandleConfirmClick;
        var rowStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = DialogMetrics.ButtonSpacing,
        };
        rowStackPanel.AddChild(cancelButton);
        rowStackPanel.AddChild(confirmButton);
        return rowStackPanel;
    }

    private void HandleCancelClick(object? sender, EventArgs e)
    {
        _sheet.Close();
    }

    private async void HandleConfirmClick(object? sender, EventArgs e)
    {
        try
        {
            await _room.ConfirmSheet();
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("The room settings could not be sent because the connection is closed.", ex);
        }
    }
}
