using System;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;
using log4net;

namespace Bastion.Client.Screens.MainMenu.Friends;

public sealed class FoundPlayerRowView : Panel
{
    private const float RowWidth = 360;
    private const float RowHeight = 52;
    private const float ContentPaddingX = 6;
    private const float ActionButtonWidth = 150;
    private const float ActionButtonHeight = 36;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(FoundPlayerRowView));

    private readonly AddFriendViewModel _addFriend;
    private readonly FoundPlayerRowViewModel _row;

    public FoundPlayerRowView(AddFriendViewModel addFriend, FoundPlayerRowViewModel row)
    {
        ArgumentNullException.ThrowIfNull(addFriend);
        ArgumentNullException.ThrowIfNull(row);

        _addFriend = addFriend;
        _row = row;
        BindingContext = row;
        Width = RowWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Height = RowHeight;
        HeightUnits = DimensionUnitType.Absolute;
        AddChild(CreateNickname());
        AddChild(CreateActionButton());
    }

    private static Label CreateNickname()
    {
        Label nickname = ViewFactory.CreateLabel(string.Empty, TextStyles.IdentityName);
        ((LabelVisual)nickname.Visual).LocalizeText = false;
        nickname.SetBinding(nameof(Label.Text), nameof(FoundPlayerRowViewModel.Nickname));
        nickname.X = ContentPaddingX;
        ViewFactory.CenterVertically(nickname);
        return nickname;
    }

    private Button CreateActionButton()
    {
        Button button = ViewFactory.CreateButton(string.Empty, ActionButtonWidth, ActionButtonHeight);
        ButtonSkin.CreateAndAttach(button, ButtonLooks.Secondary);
        button.SetBinding(nameof(Button.Text), nameof(FoundPlayerRowViewModel.ActionKey));
        button.SetBinding(nameof(IsEnabled), nameof(FoundPlayerRowViewModel.CanPress));
        button.Anchor(Gum.Wireframe.Anchor.Right);
        button.Click += HandleActionClick;
        return button;
    }

    private async void HandleActionClick(object? sender, EventArgs e)
    {
        try
        {
            await _addFriend.SendRequest(_row);
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("The friend request could not be sent because the connection is closed.", ex);
        }
    }
}
