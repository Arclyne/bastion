using System;
using System.ComponentModel;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.GueDeriving;
using log4net;

namespace Bastion.Client.Screens.MainMenu.Rooms;

public sealed class PrivateRoomPanel : Panel
{
    private const float PanelWidth = 920;
    private const float PanelY = 114;
    private const float PaddingX = 28;
    private const float PaddingTop = 24;
    private const float PaddingBottom = 26;
    private const int HorizontalSideCount = 2;
    private const float ContentWidth = PanelWidth - (PaddingX * HorizontalSideCount);
    private const float SectionSpacing = 18;
    private const float HeaderHeight = 44;
    private const float HeaderSpacing = 12;
    private const float LeaveWidth = 190;
    private const float LeaveHeight = 40;
    private const float TopCardSpacing = 16;
    private const float CodeCardWidth = 452;
    private const float SettingsCardWidth = 396;
    private const float TopCardHeight = 172;
    private const float InnerCardRadius = 18;
    private const float InnerCardBorderWidth = 1.5f;
    private const float CodeCardPaddingX = 18;
    private const float CodeCardPaddingTop = 16;
    private const float CodeCardContentWidth = CodeCardWidth - (CodeCardPaddingX * HorizontalSideCount);
    private const float CodeCardSpacing = 12;
    private const float CodeBoxSize = 36;
    private const float CodeBoxSpacing = 7;
    private const float CodeRowSpacing = 14;
    private const float CopyWidth = 150;
    private const float CopyHeight = 44;
    private const float SettingsPaddingX = 16;
    private const float SettingsPaddingTop = 14;
    private const float SettingsContentWidth = SettingsCardWidth - (SettingsPaddingX * HorizontalSideCount);
    private const float SettingsSpacing = 6;
    private const float SettingsRowHeight = 34;
    private const float SettingsRowRadius = 10;
    private const float SettingsNameX = 10;
    private const float SettingsValueX = 132;
    private const float ChangedChipX = 8;
    private const float ChangeWidth = 110;
    private const float SeatsSpacing = 10;
    private const float WideSeatSpacing = 8;
    private const float NarrowSeatSpacing = 16;
    private const float FooterSpacing = 16;
    private const float FooterButtonWidth = 280;
    private const float FooterButtonHeight = 52;
    private const int TwoSeats = 2;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(PrivateRoomPanel));

    private readonly PrivateRoomViewModel _room;
    private readonly RoomCodeBoxes _codeBoxes;

    public PrivateRoomPanel(PrivateRoomViewModel room)
    {
        ArgumentNullException.ThrowIfNull(room);

        _room = room;
        BindingContext = room;
        Width = PanelWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Height = PaddingBottom;
        HeightUnits = DimensionUnitType.RelativeToChildren;
        Anchor(Gum.Wireframe.Anchor.Top);
        Y = PanelY;
        SetBinding(nameof(IsVisible), nameof(PrivateRoomViewModel.IsInRoom));
        _codeBoxes = new RoomCodeBoxes(CodeBoxSize, TextStyleCatalog.RoomCodeLetter, CodeBoxSpacing);
        var contentStackPanel = new StackPanel
        {
            X = PaddingX,
            Y = PaddingTop,
            Width = ContentWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Spacing = SectionSpacing,
        };
        contentStackPanel.AddChild(CreateHeader());
        contentStackPanel.AddChild(CreateTopCards());
        contentStackPanel.AddChild(CreateSeats());
        contentStackPanel.AddChild(CreateFooter());
        contentStackPanel.AddChild(CreateError());
        AddChild(contentStackPanel);
        CardSkin.Apply(Visual);
        room.PropertyChanged += HandleRoomPropertyChanged;
    }

    private static Panel CreateInnerCard(float width)
    {
        var cardPanel = new Panel
        {
            Width = width,
            WidthUnits = DimensionUnitType.Absolute,
            Height = TopCardHeight,
            HeightUnits = DimensionUnitType.Absolute,
        };
        RectangleRuntime background = ShapeFactory.CreateRounded(Palette.White, InnerCardRadius);
        background.StrokeColor = Palette.Border;
        background.StrokeWidth = InnerCardBorderWidth;
        ShapeFactory.InsertBehindContent(cardPanel.Visual, background);
        return cardPanel;
    }

    private static Panel CreateSettingsRow(string nameKey, string valueProperty, string changedProperty)
    {
        var rowPanel = new Panel
        {
            Width = SettingsContentWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Height = SettingsRowHeight,
            HeightUnits = DimensionUnitType.Absolute,
        };
        var highlightPanel = new Panel();
        highlightPanel.Dock(Gum.Wireframe.Dock.Fill);
        highlightPanel.SetBinding(nameof(IsVisible), changedProperty);
        RectangleRuntime highlight = ShapeFactory.CreateRounded(Palette.WarningSoft, SettingsRowRadius);
        ShapeFactory.InsertBehindContent(highlightPanel.Visual, highlight);
        Label nameLabel = ViewFactory.CreateLabel(nameKey, TextStyleCatalog.IdentityDetail);
        nameLabel.X = SettingsNameX;
        ViewFactory.CenterVertically(nameLabel);
        Label valueLabel = ViewFactory.CreateBoundUntranslatedLabel(valueProperty, TextStyleCatalog.RoomValue);
        valueLabel.X = SettingsValueX;
        ViewFactory.CenterVertically(valueLabel);
        Panel changedChipPanel = RoomChip.Create(
            nameof(Strings.RoomChangedChip),
            TextStyleCatalog.RoomWaitingChip,
            Palette.White);
        changedChipPanel.Anchor(Gum.Wireframe.Anchor.Right);
        changedChipPanel.X = -ChangedChipX;
        changedChipPanel.SetBinding(nameof(IsVisible), changedProperty);
        rowPanel.AddChild(highlightPanel);
        rowPanel.AddChild(nameLabel);
        rowPanel.AddChild(valueLabel);
        rowPanel.AddChild(changedChipPanel);
        return rowPanel;
    }

    private Panel CreateHeader()
    {
        var headerPanel = new Panel
        {
            Width = ContentWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Height = HeaderHeight,
            HeightUnits = DimensionUnitType.Absolute,
        };
        var titleStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = HeaderSpacing,
        };
        Label titleLabel = ViewFactory.CreateLabel(nameof(Strings.RoomTitle), TextStyleCatalog.RoomTitle);
        ViewFactory.CenterVertically(titleLabel);
        Panel friendlyChipPanel = RoomChip.Create(
            nameof(Strings.RoomFriendlyChip),
            TextStyleCatalog.RoomFriendlyChip,
            Palette.InfoSoft);
        ViewFactory.CenterVertically(friendlyChipPanel);
        titleStackPanel.AddChild(titleLabel);
        titleStackPanel.AddChild(friendlyChipPanel);
        ViewFactory.CenterVertically(titleStackPanel);
        Button leaveButton = ViewFactory.CreateBoundButton(
            nameof(PrivateRoomViewModel.LeaveButtonKey),
            LeaveWidth,
            LeaveHeight);
        ButtonSkin.CreateAndAttach(leaveButton, ButtonLookCatalog.Secondary);
        leaveButton.Anchor(Gum.Wireframe.Anchor.Right);
        leaveButton.Click += HandleLeaveClick;
        headerPanel.AddChild(titleStackPanel);
        headerPanel.AddChild(leaveButton);
        return headerPanel;
    }

    private async void HandleLeaveClick(object? sender, EventArgs e)
    {
        try
        {
            await _room.Leave();
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("The room could not be left because the connection is closed.", ex);
        }
    }

    private StackPanel CreateTopCards()
    {
        var rowStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = TopCardSpacing,
        };
        rowStackPanel.AddChild(CreateCodeCard());
        rowStackPanel.AddChild(CreateSettingsCard());
        return rowStackPanel;
    }

    private Panel CreateCodeCard()
    {
        Panel cardPanel = CreateInnerCard(CodeCardWidth);
        var contentStackPanel = new StackPanel
        {
            X = CodeCardPaddingX,
            Y = CodeCardPaddingTop,
            Spacing = CodeCardSpacing,
        };
        Label titleLabel = ViewFactory.CreateLabel(nameof(Strings.RoomCodeLabel), TextStyleCatalog.SectionLabel);
        var codeRowStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = CodeRowSpacing,
        };
        Button copyButton = ViewFactory.CreateBoundButton(
            nameof(PrivateRoomViewModel.CopyButtonKey),
            CopyWidth,
            CopyHeight);
        ButtonSkin.CreateAndAttach(copyButton, ButtonLookCatalog.Secondary);
        copyButton.Click += HandleCopyClick;
        codeRowStackPanel.AddChild(_codeBoxes);
        codeRowStackPanel.AddChild(copyButton);
        Label hintLabel = ViewFactory.CreateLabel(nameof(Strings.RoomCodeHint), TextStyleCatalog.Hint);
        ViewFactory.SetFixedWidth(hintLabel, CodeCardContentWidth);
        contentStackPanel.AddChild(titleLabel);
        contentStackPanel.AddChild(codeRowStackPanel);
        contentStackPanel.AddChild(hintLabel);
        cardPanel.AddChild(contentStackPanel);
        return cardPanel;
    }

    private void HandleCopyClick(object? sender, EventArgs e)
    {
        _room.CopyCode();
    }

    private Panel CreateSettingsCard()
    {
        Panel cardPanel = CreateInnerCard(SettingsCardWidth);
        var contentStackPanel = new StackPanel
        {
            X = SettingsPaddingX,
            Y = SettingsPaddingTop,
            Spacing = SettingsSpacing,
        };
        contentStackPanel.AddChild(CreateSettingsHeading());
        contentStackPanel.AddChild(CreateSettingsRow(
            nameof(Strings.RoomModeLabel),
            nameof(PrivateRoomViewModel.ModeText),
            nameof(PrivateRoomViewModel.IsModeChanged)));
        contentStackPanel.AddChild(CreateSettingsRow(
            nameof(Strings.RoomWallsLabel),
            nameof(PrivateRoomViewModel.WallsText),
            nameof(PrivateRoomViewModel.IsWallsChanged)));
        contentStackPanel.AddChild(CreateSettingsRow(
            nameof(Strings.RoomClockLabel),
            nameof(PrivateRoomViewModel.ClockText),
            nameof(PrivateRoomViewModel.IsClockChanged)));
        cardPanel.AddChild(contentStackPanel);
        return cardPanel;
    }

    private Panel CreateSettingsHeading()
    {
        var headingPanel = new Panel
        {
            Width = SettingsContentWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Height = SettingsRowHeight,
            HeightUnits = DimensionUnitType.Absolute,
        };
        Label titleLabel = ViewFactory.CreateLabel(nameof(Strings.RoomSettingsLabel), TextStyleCatalog.SectionLabel);
        ViewFactory.CenterVertically(titleLabel);
        Button changeButton = ViewFactory.CreateButton(
            nameof(Strings.RoomChangeSettings),
            ChangeWidth,
            SettingsRowHeight);
        ButtonSkin.CreateAndAttach(changeButton, ButtonLookCatalog.Secondary);
        changeButton.Anchor(Gum.Wireframe.Anchor.Right);
        changeButton.SetBinding(nameof(IsVisible), nameof(PrivateRoomViewModel.IsHost));
        changeButton.Click += HandleChangeClick;
        headingPanel.AddChild(titleLabel);
        headingPanel.AddChild(changeButton);
        return headingPanel;
    }

    private void HandleChangeClick(object? sender, EventArgs e)
    {
        _room.OpenChangeSheet();
    }

    private StackPanel CreateSeats()
    {
        var seatsStackPanel = new StackPanel
        {
            Spacing = SeatsSpacing,
        };
        Label playersLabel = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(PrivateRoomViewModel.PlayersText),
            TextStyleCatalog.SectionLabel);
        seatsStackPanel.AddChild(playersLabel);
        seatsStackPanel.AddChild(CreateSeatRow(TwoSeats, WideSeatSpacing));
        seatsStackPanel.AddChild(CreateSeatRow(PrivateRoomViewModel.MaximumSeatCount, NarrowSeatSpacing));
        return seatsStackPanel;
    }

    private Panel CreateSeatRow(int seatCount, float spacing)
    {
        bool isWide = seatCount == TwoSeats;
        var rowStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = spacing,
        };
        for (int seatIndex = 0; seatIndex < seatCount; seatIndex++)
        {
            var seatCard = new RoomSeatCard(_room, _room.Seats[seatIndex], isWide);
            rowStackPanel.AddChild(seatCard);
        }

        string visibilityProperty = isWide
            ? nameof(PrivateRoomViewModel.IsTwoSeatLayout)
            : nameof(PrivateRoomViewModel.IsFourSeatLayout);
        return ViewFactory.CreateToggledContainer(rowStackPanel, visibilityProperty);
    }

    private StackPanel CreateFooter()
    {
        var footerStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = FooterSpacing,
        };
        Button startButton = ViewFactory.CreateBoundButton(
            nameof(PrivateRoomViewModel.StartButtonKey),
            FooterButtonWidth,
            FooterButtonHeight);
        ButtonSkin.CreateAndAttach(startButton, ButtonLookCatalog.Primary);
        startButton.SetBinding(nameof(IsVisible), nameof(PrivateRoomViewModel.IsStartShown));
        startButton.SetBinding(nameof(IsEnabled), nameof(PrivateRoomViewModel.CanStart));
        startButton.Click += HandleStartClick;
        Button readyButton = CreateReadyButton(nameof(Strings.RoomReady), ButtonLookCatalog.Primary);
        readyButton.SetBinding(nameof(IsVisible), nameof(PrivateRoomViewModel.IsReadyShown));
        Button notReadyButton = CreateReadyButton(nameof(Strings.RoomNotReady), ButtonLookCatalog.Secondary);
        notReadyButton.SetBinding(nameof(IsVisible), nameof(PrivateRoomViewModel.IsNotReadyShown));
        Label positiveLabel = ViewFactory.CreateBoundNotice(
            nameof(PrivateRoomViewModel.FooterText),
            nameof(PrivateRoomViewModel.IsFooterPositive),
            TextStyleCatalog.RoomFooterPositive);
        ViewFactory.CenterVertically(positiveLabel);
        Label neutralLabel = ViewFactory.CreateBoundNotice(
            nameof(PrivateRoomViewModel.FooterText),
            nameof(PrivateRoomViewModel.IsFooterNeutral),
            TextStyleCatalog.RoomFooterNeutral);
        ViewFactory.CenterVertically(neutralLabel);
        footerStackPanel.AddChild(startButton);
        footerStackPanel.AddChild(readyButton);
        footerStackPanel.AddChild(notReadyButton);
        footerStackPanel.AddChild(positiveLabel);
        footerStackPanel.AddChild(neutralLabel);
        return footerStackPanel;
    }

    private async void HandleStartClick(object? sender, EventArgs e)
    {
        try
        {
            await _room.StartMatch();
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("The room match could not be started because the connection is closed.", ex);
        }
    }

    private Button CreateReadyButton(string textKey, ButtonLook buttonLook)
    {
        Button button = ViewFactory.CreateButton(textKey, FooterButtonWidth, FooterButtonHeight);
        ButtonSkin.CreateAndAttach(button, buttonLook);
        button.SetBinding(nameof(IsEnabled), nameof(PrivateRoomViewModel.CanSend));
        button.Click += HandleReadyClick;
        return button;
    }

    private async void HandleReadyClick(object? sender, EventArgs e)
    {
        try
        {
            await _room.ToggleReady();
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("The ready mark could not be sent because the connection is closed.", ex);
        }
    }

    private static Label CreateError()
    {
        Label errorLabel = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(PrivateRoomViewModel.ErrorText),
            TextStyleCatalog.FieldError,
            ContentWidth);
        errorLabel.SetBinding(nameof(IsVisible), nameof(PrivateRoomViewModel.HasError));
        return errorLabel;
    }

    private void HandleRoomPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(PrivateRoomViewModel.Code))
        {
            return;
        }

        _codeBoxes.ShowCode(_room.Code);
    }
}
