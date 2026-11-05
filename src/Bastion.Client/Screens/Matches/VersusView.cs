using System;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Bastion.Domain.Matches;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.GueDeriving;

namespace Bastion.Client.Screens.Matches;

public sealed class VersusView : Panel
{
    private const float CardWidth = 560;
    private const float CardPaddingX = 40;
    private const float CardPaddingTop = 32;
    private const float CardPaddingBottom = 36;
    private const float ContentWidth = 480;
    private const float SectionSpacing = 28;
    private const float PlayerWidth = 180;
    private const float PlayerSpacing = 8;
    private const float AvatarSize = 96;
    private const float AvatarRingWidth = 3;
    private const float VersusWidth = 120;
    private const int AvatarHalves = 2;

    public VersusView(VersusViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        BindingContext = viewModel;
        Dock(Gum.Wireframe.Dock.Fill);

        var content = new StackPanel
        {
            X = CardPaddingX,
            Y = CardPaddingTop,
            Width = ContentWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Spacing = SectionSpacing,
        };
        content.AddChild(ViewFactory.CreateUntranslatedLabel(viewModel.ModeText, TextStyles.SectionLabel));
        content.AddChild(CreatePlayersRow(viewModel));
        content.AddChild(ViewFactory.CreateBoundUntranslatedLabel(
            nameof(VersusViewModel.CountdownText),
            TextStyles.DialogTitle,
            ContentWidth));

        var card = new Panel
        {
            Width = CardWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Height = CardPaddingBottom,
            HeightUnits = DimensionUnitType.RelativeToChildren,
        };
        card.Anchor(Gum.Wireframe.Anchor.Center);
        card.AddChild(content);
        Skins.ApplyCard(card.Visual);
        AddChild(card);
    }

    private static StackPanel CreatePlayersRow(VersusViewModel viewModel)
    {
        Label versus = ViewFactory.CreateLabel(nameof(Strings.VersusLabel), TextStyles.MenuTitle);
        versus.Width = VersusWidth;
        versus.WidthUnits = DimensionUnitType.Absolute;
        ViewFactory.CenterVertically(versus);

        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
        };
        row.AddChild(CreatePlayerCard(viewModel.OwnNickname, viewModel.OwnEloText, viewModel.OwnPawnSymbol));
        row.AddChild(versus);
        row.AddChild(CreatePlayerCard(viewModel.RivalNickname, viewModel.RivalEloText, viewModel.RivalPawnSymbol));
        return row;
    }

    private static StackPanel CreatePlayerCard(string nickname, string eloText, PawnSymbol pawnSymbol)
    {
        var avatar = new Panel
        {
            Width = AvatarSize,
            WidthUnits = DimensionUnitType.Absolute,
            Height = AvatarSize,
            HeightUnits = DimensionUnitType.Absolute,
        };
        RectangleRuntime circle = Shapes.CreateRounded(Palette.AvatarFill, AvatarSize / AvatarHalves);
        circle.StrokeColor = Palette.GetPawnColor(pawnSymbol);
        circle.StrokeWidth = AvatarRingWidth;
        Shapes.InsertBehindContent(avatar.Visual, circle);

        var player = new StackPanel
        {
            Width = PlayerWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Spacing = PlayerSpacing,
        };
        player.AddChild(avatar);
        player.AddChild(ViewFactory.CreateUntranslatedLabel(nickname, TextStyles.IdentityName));
        player.AddChild(ViewFactory.CreateUntranslatedLabel(eloText, TextStyles.IdentityDetail));
        return player;
    }
}
