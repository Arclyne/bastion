using System;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Bastion.Domain.Matches;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Microsoft.Xna.Framework;

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
        Label mode = ViewFactory.CreateUntranslatedLabel(viewModel.ModeText, TextStyleCatalog.SectionLabel);
        StackPanel playersRow = CreatePlayersRow(viewModel);
        Label countdown = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(VersusViewModel.CountdownText),
            TextStyleCatalog.DialogTitle,
            contentWidth);
        content.AddChild(mode);
        content.AddChild(playersRow);
        content.AddChild(countdown);

        float cardWidth = contentWidth + (CardPaddingX * HorizontalSideCount);
        var card = new Panel
        {
            Width = cardWidth,
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
        ViewFactory.SetFixedWidth(versus, VersusWidth);
        ViewFactory.CenterVertically(versus);

        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
        };
        StackPanel ownCard = CreatePlayerCard(viewModel.Own);
        row.AddChild(ownCard);
        row.AddChild(versus);
        foreach (VersusPlayer rival in viewModel.Rivals)
        {
            StackPanel rivalCard = CreatePlayerCard(rival);
            row.AddChild(rivalCard);
        }

        return row;
    }

    private static StackPanel CreatePlayerCard(VersusPlayer player)
    {
        Color ringColor = Palette.GetPawnColor(player.PawnSymbol);
        Panel avatar = ShapeFactory.CreateAvatar(AvatarSize, ringColor, AvatarRingWidth);

        var card = new StackPanel
        {
            Width = PlayerWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Spacing = PlayerSpacing,
        };
        Label nickname = ViewFactory.CreateUntranslatedLabel(player.Nickname, TextStyleCatalog.IdentityName);
        Label elo = ViewFactory.CreateUntranslatedLabel(player.EloText, TextStyleCatalog.IdentityDetail);
        card.AddChild(avatar);
        card.AddChild(nickname);
        card.AddChild(elo);
        return card;
    }
}
