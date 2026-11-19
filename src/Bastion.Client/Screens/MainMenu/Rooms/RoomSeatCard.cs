using System;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Bastion.Domain.Matches;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.GueDeriving;
using Microsoft.Xna.Framework;

namespace Bastion.Client.Screens.MainMenu.Rooms;

public sealed class RoomSeatCard : Panel
{
    private const float WideWidth = 428;
    private const float WideHeight = 120;
    private const float NarrowWidth = 204;
    private const float NarrowHeight = 184;
    private const float CardRadius = 18;
    private const float CardBorderWidth = 1.5f;
    private const float FreeBorderWidth = 2;
    private const float WideAvatarSize = 56;
    private const float NarrowAvatarSize = 50;
    private const float AvatarRingWidth = 3;
    private const float FreeRingWidth = 2;
    private const float PaddingX = 18;
    private const float WideSpacing = 14;
    private const float NarrowSpacing = 8;
    private const float TextSpacing = 7;
    private const float ChipSpacing = 6;
    private const float KickWidth = 124;
    private const float WideKickHeight = 36;
    private const float NarrowKickHeight = 32;

    private static readonly PawnSymbol[] _seatSymbols =
    {
        PawnSymbol.Red,
        PawnSymbol.Green,
        PawnSymbol.Blue,
        PawnSymbol.Yellow,
    };

    private readonly PrivateRoomViewModel _room;
    private readonly RoomSeatViewModel _seat;
    private readonly bool _isWide;

    public RoomSeatCard(PrivateRoomViewModel room, RoomSeatViewModel seat, bool isWide)
    {
        ArgumentNullException.ThrowIfNull(room);
        ArgumentNullException.ThrowIfNull(seat);

        _room = room;
        _seat = seat;
        _isWide = isWide;
        BindingContext = seat;
        Width = isWide ? WideWidth : NarrowWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Height = isWide ? WideHeight : NarrowHeight;
        HeightUnits = DimensionUnitType.Absolute;
        Panel takenPanel = CreateTakenSeat();
        Panel freePanel = CreateFreeSeat();
        AddChild(takenPanel);
        AddChild(freePanel);
    }

    private static Panel CreateLayer(string visibilityProperty, RectangleRuntime background)
    {
        var layerPanel = new Panel();
        layerPanel.Dock(Gum.Wireframe.Dock.Fill);
        layerPanel.SetBinding(nameof(IsVisible), visibilityProperty);
        ShapeFactory.InsertBehindContent(layerPanel.Visual, background);
        return layerPanel;
    }

    private static Panel CreateChips()
    {
        var chipsStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = ChipSpacing,
        };
        Panel hostChipPanel = RoomChip.Create(
            nameof(Strings.RoomHostChip),
            TextStyleCatalog.RoomHostChip,
            Palette.RejoinSoft);
        hostChipPanel.SetBinding(nameof(IsVisible), nameof(RoomSeatViewModel.IsHost));
        Panel readyChipPanel = RoomChip.Create(
            nameof(Strings.RoomReadyChip),
            TextStyleCatalog.RoomReadyChip,
            Palette.SuccessSoft);
        readyChipPanel.SetBinding(nameof(IsVisible), nameof(RoomSeatViewModel.IsReady));
        Panel waitingChipPanel = RoomChip.Create(
            nameof(Strings.RoomNotReadyChip),
            TextStyleCatalog.RoomWaitingChip,
            Palette.WarningSoft);
        waitingChipPanel.SetBinding(nameof(IsVisible), nameof(RoomSeatViewModel.IsNotReady));
        chipsStackPanel.AddChild(hostChipPanel);
        chipsStackPanel.AddChild(readyChipPanel);
        chipsStackPanel.AddChild(waitingChipPanel);
        return WrapRow(chipsStackPanel);
    }

    private static Panel WrapRow(FrameworkElement row)
    {
        var rowPanel = new Panel
        {
            WidthUnits = DimensionUnitType.RelativeToChildren,
            HeightUnits = DimensionUnitType.RelativeToChildren,
        };
        rowPanel.AddChild(row);
        return rowPanel;
    }

    private Panel CreateTakenSeat()
    {
        RectangleRuntime background = ShapeFactory.CreateRounded(Palette.White, CardRadius);
        background.StrokeColor = Palette.Border;
        background.StrokeWidth = CardBorderWidth;
        Panel layerPanel = CreateLayer(nameof(RoomSeatViewModel.IsTaken), background);
        Color seatColor = Palette.GetPawnColor(_seatSymbols[_seat.SeatIndex]);
        float avatarSize = _isWide ? WideAvatarSize : NarrowAvatarSize;
        Panel avatarPanel = ShapeFactory.CreateAvatar(avatarSize, seatColor, AvatarRingWidth);
        Label nameLabel = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(RoomSeatViewModel.NameText),
            TextStyleCatalog.RoomSeatName);
        Panel chipsPanel = CreateChips();
        Button kickButton = CreateKickButton();
        if (_isWide)
        {
            var textStackPanel = new StackPanel
            {
                Spacing = TextSpacing,
            };
            textStackPanel.AddChild(nameLabel);
            textStackPanel.AddChild(chipsPanel);
            ViewFactory.CenterVertically(textStackPanel);
            AddWideContent(layerPanel, avatarPanel, textStackPanel);
            kickButton.Anchor(Gum.Wireframe.Anchor.Right);
            kickButton.X = -PaddingX;
            layerPanel.AddChild(kickButton);
            return layerPanel;
        }

        var columnStackPanel = new StackPanel
        {
            Spacing = NarrowSpacing,
        };
        columnStackPanel.AddChild(CenterInColumn(avatarPanel));
        columnStackPanel.AddChild(CenterInColumn(nameLabel));
        columnStackPanel.AddChild(CenterInColumn(chipsPanel));
        columnStackPanel.AddChild(CenterInColumn(kickButton));
        ViewFactory.CenterVertically(columnStackPanel);
        layerPanel.AddChild(columnStackPanel);
        return layerPanel;
    }

    private Button CreateKickButton()
    {
        float height = _isWide ? WideKickHeight : NarrowKickHeight;
        Button button = ViewFactory.CreateButton(nameof(Strings.RoomKick), KickWidth, height);
        ButtonSkin.CreateAndAttach(button, ButtonLookCatalog.Danger);
        button.SetBinding(nameof(IsVisible), nameof(RoomSeatViewModel.IsKickable));
        button.Click += HandleKickClick;
        return button;
    }

    private void HandleKickClick(object? sender, EventArgs e)
    {
        _room.AskToKick(_seat.Nickname);
    }

    private static void AddWideContent(Panel layerPanel, Panel avatarPanel, StackPanel textStackPanel)
    {
        var rowStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = WideSpacing,
            X = PaddingX,
        };
        ViewFactory.CenterVertically(avatarPanel);
        rowStackPanel.AddChild(avatarPanel);
        rowStackPanel.AddChild(textStackPanel);
        ViewFactory.CenterVertically(rowStackPanel);
        layerPanel.AddChild(rowStackPanel);
    }

    private Panel CenterInColumn(FrameworkElement element)
    {
        ViewFactory.CenterHorizontally(element);
        var rowPanel = new Panel
        {
            Width = Width,
            WidthUnits = DimensionUnitType.Absolute,
            HeightUnits = DimensionUnitType.RelativeToChildren,
        };
        rowPanel.AddChild(element);
        return rowPanel;
    }

    private Panel CreateFreeSeat()
    {
        RectangleRuntime background = ShapeFactory.CreateRounded(Palette.Background, CardRadius);
        background.StrokeColor = Palette.BorderStrong;
        background.StrokeWidth = FreeBorderWidth;
        Panel layerPanel = CreateLayer(nameof(RoomSeatViewModel.IsFree), background);
        float avatarSize = _isWide ? WideAvatarSize : NarrowAvatarSize;
        Panel avatarPanel = ShapeFactory.CreateAvatar(avatarSize, Palette.BorderStrong, FreeRingWidth);
        Label titleLabel = ViewFactory.CreateLabel(nameof(Strings.RoomFreeSeat), TextStyleCatalog.RoomFreeSeat);
        if (!_isWide)
        {
            var columnStackPanel = new StackPanel
            {
                Spacing = NarrowSpacing,
            };
            columnStackPanel.AddChild(CenterInColumn(avatarPanel));
            columnStackPanel.AddChild(CenterInColumn(titleLabel));
            ViewFactory.CenterVertically(columnStackPanel);
            layerPanel.AddChild(columnStackPanel);
            return layerPanel;
        }

        Label hintLabel = ViewFactory.CreateLabel(nameof(Strings.RoomFreeSeatHint), TextStyleCatalog.Hint);
        var textStackPanel = new StackPanel
        {
            Spacing = TextSpacing,
        };
        textStackPanel.AddChild(titleLabel);
        textStackPanel.AddChild(hintLabel);
        ViewFactory.CenterVertically(textStackPanel);
        AddWideContent(layerPanel, avatarPanel, textStackPanel);
        return layerPanel;
    }
}
