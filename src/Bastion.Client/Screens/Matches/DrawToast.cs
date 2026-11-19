using System;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.GueDeriving;

namespace Bastion.Client.Screens.Matches;

public sealed class DrawToast : Panel
{
    private const float ToastWidth = 520;
    private const float PaddingX = 16;
    private const float PaddingY = 12;
    private const float ToastRadius = 16;
    private const int SideCount = 2;
    private const float TextWidth = ToastWidth - (PaddingX * SideCount);

    public DrawToast(MatchDrawViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        BindingContext = viewModel;
        Width = ToastWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Height = PaddingY * SideCount;
        HeightUnits = DimensionUnitType.RelativeToChildren;
        SetBinding(nameof(IsVisible), nameof(MatchDrawViewModel.HasToast));
        RectangleRuntime background = ShapeFactory.CreateRounded(Palette.InfoSoft, ToastRadius);
        ShapeFactory.InsertBehindContent(Visual, background);
        Label messageLabel = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(MatchDrawViewModel.ToastText),
            TextStyleCatalog.NoticeMessage,
            TextWidth);
        messageLabel.X = PaddingX;
        messageLabel.Y = PaddingY;
        AddChild(messageLabel);
    }
}
