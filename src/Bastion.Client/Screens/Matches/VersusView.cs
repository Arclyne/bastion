using System;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
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

        var contentStackPanel = new StackPanel
        {
            X = CardPaddingX,
            Y = CardPaddingTop,
            Width = contentWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Spacing = SectionSpacing,
        };
        Label modeLabel = ViewFactory.CreateUntranslatedLabel(viewModel.ModeText, TextStyleCatalog.SectionLabel);
        StackPanel playersRowStackPanel = CreatePlayersRow(viewModel);
        Label countdownLabel = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(VersusViewModel.CountdownText),
            TextStyleCatalog.DialogTitle,
            contentWidth);
        contentStackPanel.AddChild(modeLabel);
        contentStackPanel.AddChild(playersRowStackPanel);
        contentStackPanel.AddChild(countdownLabel);

        float cardWidth = contentWidth + (CardPaddingX * HorizontalSideCount);
        var cardPanel = new Panel
        {
            Width = cardWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Height = CardPaddingBottom,
            HeightUnits = DimensionUnitType.RelativeToChildren,
        };
        cardPanel.Anchor(Gum.Wireframe.Anchor.Center);
        cardPanel.AddChild(contentStackPanel);
        CardSkin.Apply(cardPanel.Visual);
        AddChild(cardPanel);
    }

    private static StackPanel CreatePlayersRow(VersusViewModel viewModel)
    {
        Label versusLabel = ViewFactory.CreateLabel(nameof(Strings.VersusLabel), TextStyleCatalog.MenuTitle);
        ViewFactory.SetFixedWidth(versusLabel, VersusWidth);
        ViewFactory.CenterVertically(versusLabel);

        var rowStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
        };
        StackPanel ownCardStackPanel = CreatePlayerCard(viewModel.Own);
        rowStackPanel.AddChild(ownCardStackPanel);
        rowStackPanel.AddChild(versusLabel);
        foreach (VersusPlayer rival in viewModel.Rivals)
        {
            StackPanel rivalCardStackPanel = CreatePlayerCard(rival);
            rowStackPanel.AddChild(rivalCardStackPanel);
        }

        return rowStackPanel;
    }

    private static StackPanel CreatePlayerCard(VersusPlayer player)
    {
        Color ringColor = Palette.GetPawnColor(player.PawnSymbol);
        Panel avatarPanel = ShapeFactory.CreateAvatar(AvatarSize, ringColor, AvatarRingWidth);

        var cardStackPanel = new StackPanel
        {
            Width = PlayerWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Spacing = PlayerSpacing,
        };
        Label nicknameLabel = ViewFactory.CreateUntranslatedLabel(player.Nickname, TextStyleCatalog.IdentityName);
        Label eloLabel = ViewFactory.CreateUntranslatedLabel(player.EloText, TextStyleCatalog.IdentityDetail);
        cardStackPanel.AddChild(avatarPanel);
        cardStackPanel.AddChild(nicknameLabel);
        cardStackPanel.AddChild(eloLabel);
        return cardStackPanel;
    }
}
