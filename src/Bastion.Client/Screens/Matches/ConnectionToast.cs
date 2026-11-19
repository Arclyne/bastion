using System;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.GueDeriving;
using Microsoft.Xna.Framework;

namespace Bastion.Client.Screens.Matches;

public sealed class ConnectionToast : Panel
{
    private const float ToastWidth = 520;
    private const float PaddingX = 16;
    private const float PaddingY = 12;
    private const float ToastRadius = 16;
    private const int SideCount = 2;
    private const float TextWidth = ToastWidth - (PaddingX * SideCount);

    public ConnectionToast(MatchConnectionViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        BindingContext = viewModel;
        Width = ToastWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Height = 0;
        HeightUnits = DimensionUnitType.RelativeToChildren;
        SetBinding(nameof(IsVisible), nameof(MatchConnectionViewModel.HasToast));
        Panel successPanel = CreateTone(nameof(MatchConnectionViewModel.HasSuccessToast), Palette.SuccessSoft);
        Panel informationPanel = CreateTone(nameof(MatchConnectionViewModel.HasInformationToast), Palette.InfoSoft);
        AddChild(successPanel);
        AddChild(informationPanel);
    }

    private static Panel CreateTone(string visibilityProperty, Color fillColor)
    {
        var tonePanel = new Panel
        {
            Width = ToastWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Height = PaddingY * SideCount,
            HeightUnits = DimensionUnitType.RelativeToChildren,
        };
        tonePanel.SetBinding(nameof(IsVisible), visibilityProperty);
        RectangleRuntime background = ShapeFactory.CreateRounded(fillColor, ToastRadius);
        ShapeFactory.InsertBehindContent(tonePanel.Visual, background);
        Label messageLabel = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(MatchConnectionViewModel.ToastText),
            TextStyleCatalog.NoticeMessage,
            TextWidth);
        messageLabel.X = PaddingX;
        messageLabel.Y = PaddingY;
        tonePanel.AddChild(messageLabel);
        return tonePanel;
    }
}
