using System;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Bastion.Domain.Matches;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.GueDeriving;

namespace Bastion.Client.Screens.Matches;

public sealed class VersusView : ScreenView
{
    private const float CardPaddingX = 40;
    private const float CardPaddingTop = 32;
    private const float CardPaddingBottom = 36;
    private const float SectionSpacing = 28;
    private const float PlayerWidth = 180;
    private const float PlayerSpacing = 8;
    private const float AvatarSize = 96;
    private const float AvatarRingWidth = 3;
    private const float VersusWidth = 120;
    private const int HorizontalSideCount = 2;
    private const int OwnPlayerCount = 1;

    public VersusView(VersusViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        BindingContext = viewModel;
        float contentWidth = ((OwnPlayerCount + viewModel.Rivals.Count) * PlayerWidth) + VersusWidth;

        var content = new StackPanel
        {
            X = CardPaddingX,
            Y = CardPaddingTop,
            Width = contentWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Spacing = SectionSpacing,
        };
        content.AddChild(ViewFactory.CreateUntranslatedLabel(viewModel.ModeText, TextStyleCatalog.SectionLabel));
        content.AddChild(CreatePlayersRow(viewModel));
        content.AddChild(ViewFactory.CreateBoundUntranslatedLabel(
            nameof(VersusViewModel.CountdownText),
            TextStyleCatalog.DialogTitle,
            contentWidth));

        var card = new Panel
        {
            Width = contentWidth + (CardPaddingX * HorizontalSideCount),
            WidthUnits = DimensionUnitType.Absolute,
            Height = CardPaddingBottom,
            HeightUnits = DimensionUnitType.RelativeToChildren,
        };
        card.Anchor(Gum.Wireframe.Anchor.Center);
        card.AddChild(content);
        CardSkin.Apply(card.Visual);
        AddChild(card);
    }

    private static StackPanel CreatePlayersRow(VersusViewModel viewModel)
    {
        Label versus = ViewFactory.CreateLabel(nameof(Strings.VersusLabel), TextStyleCatalog.MenuTitle);
        versus.Width = VersusWidth;
        versus.WidthUnits = DimensionUnitType.Absolute;
        ViewFactory.CenterVertically(versus);

        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
        };
        row.AddChild(CreatePlayerCard(viewModel.Own));
        row.AddChild(versus);
        foreach (VersusPlayer rival in viewModel.Rivals)
        {
            row.AddChild(CreatePlayerCard(rival));
        }

        return row;
    }

    private static StackPanel CreatePlayerCard(VersusPlayer player)
    {
        var avatar = new Panel
        {
            Width = AvatarSize,
            WidthUnits = DimensionUnitType.Absolute,
            Height = AvatarSize,
            HeightUnits = DimensionUnitType.Absolute,
        };
        RectangleRuntime circle = ShapeFactory.CreateCircle(Palette.AvatarFill, AvatarSize);
        circle.StrokeColor = Palette.GetPawnColor(player.PawnSymbol);
        circle.StrokeWidth = AvatarRingWidth;
        ShapeFactory.InsertBehindContent(avatar.Visual, circle);

        var card = new StackPanel
        {
            Width = PlayerWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Spacing = PlayerSpacing,
        };
        card.AddChild(avatar);
        card.AddChild(ViewFactory.CreateUntranslatedLabel(player.Nickname, TextStyleCatalog.IdentityName));
        card.AddChild(ViewFactory.CreateUntranslatedLabel(player.EloText, TextStyleCatalog.IdentityDetail));
        return card;
    }
}
