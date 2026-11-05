using System;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;

namespace Bastion.Client.Screens.Matches;

public sealed class MatchView : Panel
{
    private const float TurnPillWidth = 160;

    public MatchView(MatchViewModel matchViewModel, MatchChatViewModel chatViewModel)
    {
        ArgumentNullException.ThrowIfNull(matchViewModel);
        ArgumentNullException.ThrowIfNull(chatViewModel);

        Dock(Gum.Wireframe.Dock.Fill);
        AddChild(CreateBoard(matchViewModel));
        AddChild(CreatePlayersPanel(matchViewModel));
        AddChild(CreateTurnPill(matchViewModel));
        AddChild(CreateChatPanel(chatViewModel));
    }

    private static BoardView CreateBoard(MatchViewModel matchViewModel)
    {
        var board = new BoardView(matchViewModel);
        board.Anchor(Gum.Wireframe.Anchor.Center);
        return board;
    }

    private static MatchPlayersPanel CreatePlayersPanel(MatchViewModel matchViewModel)
    {
        var panel = new MatchPlayersPanel(matchViewModel);
        panel.Anchor(Gum.Wireframe.Anchor.TopLeft);
        panel.X = ScreenMetrics.MarginX;
        panel.Y = ScreenMetrics.MarginY;
        return panel;
    }

    private static Panel CreateTurnPill(MatchViewModel matchViewModel)
    {
        var pill = new Panel
        {
            Width = TurnPillWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Height = ScreenMetrics.MenuPillHeight,
            HeightUnits = DimensionUnitType.Absolute,
        };
        pill.Anchor(Gum.Wireframe.Anchor.TopRight);
        pill.X = -ScreenMetrics.MarginX;
        pill.Y = ScreenMetrics.MarginY;
        Label text = ViewFactory.CreateLabel(matchViewModel.TurnTextKey, TextStyles.IdentityName);
        text.Anchor(Gum.Wireframe.Anchor.Center);
        pill.AddChild(text);
        Skins.ApplyCard(pill.Visual);
        return pill;
    }

    private static MatchChatPanel CreateChatPanel(MatchChatViewModel chatViewModel)
    {
        var panel = new MatchChatPanel(chatViewModel);
        panel.Anchor(Gum.Wireframe.Anchor.BottomRight);
        panel.X = -ScreenMetrics.MarginX;
        panel.Y = -ScreenMetrics.MarginY;
        return panel;
    }
}
