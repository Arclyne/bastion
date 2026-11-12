using System;
using System.Threading.Tasks;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using log4net;

namespace Bastion.Client.Screens.MainMenu.Friends;

public sealed class FriendRequestToast : Panel
{
    private const float ToastWidth = 460;
    private const float ToastHeight = 64;
    private const float PaddingX = 18;
    private const float ButtonSpacing = 8;
    private const float ActionButtonWidth = 96;
    private const float ActionButtonHeight = 36;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(FriendRequestToast));

    private readonly FriendRequestsViewModel _requests;

    public FriendRequestToast(FriendRequestsViewModel requests)
    {
        ArgumentNullException.ThrowIfNull(requests);

        _requests = requests;
        BindingContext = requests;
        Width = ToastWidth;
        WidthUnits = DimensionUnitType.Absolute;
        Height = ToastHeight;
        HeightUnits = DimensionUnitType.Absolute;
        this.SetBinding(nameof(IsVisible), nameof(FriendRequestsViewModel.IsToastVisible));
        AddChild(CreateMessage());
        AddChild(CreateActions());
        Skins.ApplyCard(Visual);
    }

    private static Label CreateMessage()
    {
        Label message = ViewFactory.CreateBoundUntranslatedLabel(nameof(FriendRequestsViewModel.ToastText), TextStyles.NoticeTitle);
        message.X = PaddingX;
        ViewFactory.CenterVertically(message);
        return message;
    }

    private StackPanel CreateActions()
    {
        Button reject = CreateActionButton(nameof(Strings.FriendsReject), ButtonLooks.Secondary);
        reject.Click += HandleRejectClick;
        Button accept = CreateActionButton(nameof(Strings.FriendsAccept), ButtonLooks.Primary);
        accept.Click += HandleAcceptClick;

        var actions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = ButtonSpacing,
        };
        actions.AddChild(reject);
        actions.AddChild(accept);
        actions.Anchor(Gum.Wireframe.Anchor.Right);
        actions.X = -PaddingX;
        actions.SetBinding(nameof(IsVisible), nameof(FriendRequestsViewModel.AreToastActionsVisible));
        return actions;
    }

    private static Button CreateActionButton(string textKey, ButtonLook look)
    {
        Button button = ViewFactory.CreateButton(textKey, ActionButtonWidth, ActionButtonHeight);
        ButtonSkin.CreateAndAttach(button, look);
        return button;
    }

    private static async Task RespondSafely(Task response)
    {
        try
        {
            await response;
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("The friend request toast could not respond because the connection is closed.", ex);
        }
    }

    private void HandleRejectClick(object? sender, EventArgs e)
    {
        _ = RespondSafely(_requests.RejectToast());
    }

    private void HandleAcceptClick(object? sender, EventArgs e)
    {
        _ = RespondSafely(_requests.AcceptToast());
    }
}
