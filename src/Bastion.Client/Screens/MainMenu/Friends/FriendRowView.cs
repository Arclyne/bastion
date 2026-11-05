using System;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;
using Gum.GueDeriving;
using Microsoft.Xna.Framework;

namespace Bastion.Client.Screens.MainMenu.Friends;

public sealed class FriendRowView : Panel
{
    private const float RowWidth = 360;
    private const float RowHeight = 52;
    private const float DotSize = 10;
    private const float ContentPaddingX = 6;
    private const float ContentSpacing = 12;
    private const float TextSpacing = 2;

    public FriendRowView(FriendRowViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        BindingContext = viewModel;
        Width = RowWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Height = RowHeight;
        HeightUnits = DimensionUnitType.Absolute;

        var content = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = ContentSpacing,
        };
        content.AddChild(CreatePresenceDot(viewModel.IsConnected));
        content.AddChild(CreateText());
        content.Anchor(Gum.Wireframe.Anchor.Left);
        content.X = ContentPaddingX;
        AddChild(content);
    }

    private static Panel CreatePresenceDot(bool isConnected)
    {
        var dot = new Panel
        {
            Width = DotSize,
            WidthUnits = DimensionUnitType.Absolute,
            Height = DotSize,
            HeightUnits = DimensionUnitType.Absolute,
        };
        Color color = isConnected ? Palette.Success : Palette.Muted;
        RectangleRuntime circle = Shapes.CreateRounded(color, DotSize / 2);
        Shapes.InsertBehindContent(dot.Visual, circle);
        ViewFactory.CenterVertically(dot);
        return dot;
    }

    private static StackPanel CreateText()
    {
        var text = new StackPanel { Spacing = TextSpacing };
        text.AddChild(CreateBoundLabel(nameof(FriendRowViewModel.Nickname), TextStyles.IdentityName));
        text.AddChild(CreateBoundLabel(nameof(FriendRowViewModel.StatusText), TextStyles.Hint));
        ViewFactory.CenterVertically(text);
        return text;
    }

    private static Label CreateBoundLabel(string propertyName, TextStyle style)
    {
        Label label = ViewFactory.CreateLabel(string.Empty, style);
        ((LabelVisual)label.Visual).LocalizeText = false;
        label.SetBinding(nameof(Label.Text), propertyName);
        return label;
    }
}
