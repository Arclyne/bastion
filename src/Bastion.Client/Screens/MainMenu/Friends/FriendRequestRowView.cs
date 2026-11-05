using System;
using System.Threading.Tasks;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;
using log4net;

namespace Bastion.Client.Screens.MainMenu.Friends;

public sealed class FriendRequestRowView : Panel
{
    private const float RowWidth = 360;
    private const float RowHeight = 56;
    private const float ContentPaddingX = 6;
    private const float TextSpacing = 2;
    private const float ButtonSpacing = 6;
    private const float ActionButtonWidth = 92;
    private const float ActionButtonHeight = 34;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(FriendRequestRowView));

    private readonly FriendRequestsViewModel _requests;
    private readonly FriendRequestRowViewModel _row;

    public FriendRequestRowView(FriendRequestsViewModel requests, FriendRequestRowViewModel row)
    {
        ArgumentNullException.ThrowIfNull(requests);
        ArgumentNullException.ThrowIfNull(row);

        _requests = requests;
        _row = row;
        BindingContext = row;
        Width = RowWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Height = RowHeight;
        HeightUnits = DimensionUnitType.Absolute;
        AddChild(CreateText());
        AddChild(row.IsReceived ? CreateReceivedActions() : CreateSentActions());
    }

    private static StackPanel CreateText()
    {
        var text = new StackPanel { X = ContentPaddingX, Spacing = TextSpacing };
        text.AddChild(CreateBoundLabel(nameof(FriendRequestRowViewModel.Nickname), TextStyles.IdentityName));
        text.AddChild(CreateBoundLabel(nameof(FriendRequestRowViewModel.DetailText), TextStyles.Hint));
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

    private static Button CreateActionButton(string textKey, ButtonLook look)
    {
        Button button = ViewFactory.CreateButton(textKey, ActionButtonWidth, ActionButtonHeight);
        ButtonSkin.CreateAndAttach(button, look);
        button.SetBinding(nameof(IsEnabled), nameof(FriendRequestRowViewModel.CanAct));
        return button;
    }

    private StackPanel CreateReceivedActions()
    {
        Button reject = CreateActionButton(nameof(Strings.FriendsReject), ButtonLooks.Secondary);
        reject.Click += HandleRejectClick;
        Button accept = CreateActionButton(nameof(Strings.FriendsAccept), ButtonLooks.Primary);
        accept.Click += HandleAcceptClick;
        return CreateActionRow(reject, accept);
    }

    private StackPanel CreateSentActions()
    {
        Button cancel = CreateActionButton(nameof(Strings.FriendsCancelRequest), ButtonLooks.Secondary);
        cancel.Click += HandleCancelClick;
        return CreateActionRow(cancel);
    }

    private StackPanel CreateActionRow(params Button[] buttons)
    {
        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = ButtonSpacing };
        foreach (Button button in buttons)
        {
            actions.AddChild(button);
        }

        actions.Anchor(Gum.Wireframe.Anchor.Right);
        return actions;
    }

    private void HandleAcceptClick(object? sender, EventArgs e)
    {
        RunSafely(_requests.Accept(_row));
    }

    private void HandleRejectClick(object? sender, EventArgs e)
    {
        RunSafely(_requests.Reject(_row));
    }

    private void HandleCancelClick(object? sender, EventArgs e)
    {
        RunSafely(_requests.Cancel(_row));
    }

    private async void RunSafely(Task response)
    {
        try
        {
            await response;
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("The friend request response could not be sent because the connection is closed.", ex);
        }
    }
}
