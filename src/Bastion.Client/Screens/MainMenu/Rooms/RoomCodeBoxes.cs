using System;
using System.Collections.Generic;
using Bastion.Client.Theme;
using Bastion.Domain.Rooms;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.GueDeriving;

namespace Bastion.Client.Screens.MainMenu.Rooms;

public sealed class RoomCodeBoxes : StackPanel
{
    private const float BoxRadius = 12;
    private const float BorderWidth = 2;
    private const float HeightPerWidth = 1.2f;
    private const int NoBox = -1;

    private readonly List<Label> _letterLabels;
    private readonly List<RectangleRuntime> _boxes;

    public RoomCodeBoxes(float boxSize, TextStyle letterStyle, float spacing)
    {
        ArgumentNullException.ThrowIfNull(letterStyle);

        _letterLabels = [];
        _boxes = [];
        Orientation = Orientation.Horizontal;
        Spacing = spacing;
        for (int boxIndex = 0; boxIndex < RoomPolicy.CodeLength; boxIndex++)
        {
            Panel boxPanel = CreateBox(boxSize, letterStyle);
            AddChild(boxPanel);
        }

        HighlightBox(NoBox);
    }

    public void ShowCode(string code)
    {
        ArgumentNullException.ThrowIfNull(code);

        for (int boxIndex = 0; boxIndex < _letterLabels.Count; boxIndex++)
        {
            _letterLabels[boxIndex].Text = boxIndex < code.Length ? code[boxIndex].ToString() : string.Empty;
        }
    }

    public void HighlightBox(int highlightedIndex)
    {
        for (int boxIndex = 0; boxIndex < _boxes.Count; boxIndex++)
        {
            _boxes[boxIndex].StrokeColor = boxIndex == highlightedIndex ? Palette.Accent : Palette.Border;
        }
    }

    private Panel CreateBox(float boxSize, TextStyle letterStyle)
    {
        var boxPanel = new Panel
        {
            Width = boxSize,
            WidthUnits = DimensionUnitType.Absolute,
            Height = boxSize * HeightPerWidth,
            HeightUnits = DimensionUnitType.Absolute,
        };
        RectangleRuntime box = ShapeFactory.CreateRounded(Palette.Card, BoxRadius);
        box.StrokeWidth = BorderWidth;
        ShapeFactory.InsertBehindContent(boxPanel.Visual, box);
        Label letterLabel = ViewFactory.CreateUntranslatedLabel(string.Empty, letterStyle);
        ViewFactory.CenterHorizontally(letterLabel);
        ViewFactory.CenterVertically(letterLabel);
        boxPanel.AddChild(letterLabel);
        _boxes.Add(box);
        _letterLabels.Add(letterLabel);
        return boxPanel;
    }
}
