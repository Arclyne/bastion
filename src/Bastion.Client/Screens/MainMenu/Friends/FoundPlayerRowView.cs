using System;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using log4net;

namespace Bastion.Client.Screens.MainMenu.Friends;

public sealed class FoundPlayerRowView : Panel
{
    private const float RowWidth = PanelMetrics.ContentWidth;
    private const float ActionButtonWidth = 150;
    private const float ActionButtonHeight = 36;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(FoundPlayerRowView));

    private readonly AddFriendViewModel _addFriendViewModel;
    private readonly FoundPlayerRowViewModel _row;

    public FoundPlayerRowView(AddFriendViewModel addFriendViewModel, FoundPlayerRowViewModel row)
    {
        ArgumentNullException.ThrowIfNull(addFriendViewModel);
        ArgumentNullException.ThrowIfNull(row);

        _addFriendViewModel = addFriendViewModel;
        _row = row;
        BindingContext = row;
        Width = RowWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Height = PanelMetrics.PlayerRowHeight;
        HeightUnits = DimensionUnitType.Absolute;
        Label nicknameLabel = CreateNickname();
        Button actionButton = CreateActionButton();
        AddChild(nicknameLabel);
        AddChild(actionButton);
    }

    private static Label CreateNickname()
    {
        Label nicknameLabel = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(FoundPlayerRowViewModel.Nickname),
            TextStyleCatalog.IdentityName);
        nicknameLabel.X = PanelMetrics.RowContentPaddingX;
        ViewFactory.CenterVertically(nicknameLabel);
        return nicknameLabel;
    }

    private Button CreateActionButton()
    {
        Button button = ViewFactory.CreateBoundButton(
            nameof(FoundPlayerRowViewModel.ActionKey),
            ActionButtonWidth,
            ActionButtonHeight);
        ButtonSkin.CreateAndAttach(button, ButtonLookCatalog.Secondary);
        button.SetBinding(nameof(IsEnabled), nameof(FoundPlayerRowViewModel.CanPress));
        button.Anchor(Gum.Wireframe.Anchor.Right);
        button.Click += HandleActionClick;
        return button;
    }

    private async void HandleActionClick(object? sender, EventArgs e)
    {
        try
        {
            await _addFriendViewModel.SendRequest(_row);
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("The friend request could not be sent because the connection is closed.", ex);
        }
    }
}
