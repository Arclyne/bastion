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
        Label messageLabel = CreateMessage();
        StackPanel actionsStackPanel = CreateActions();
        AddChild(messageLabel);
        AddChild(actionsStackPanel);
        CardSkin.Apply(Visual);
    }

    private static Label CreateMessage()
    {
        Label messageLabel = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(FriendRequestsViewModel.ToastText),
            TextStyleCatalog.NoticeTitle);
        messageLabel.X = PaddingX;
        ViewFactory.CenterVertically(messageLabel);
        return messageLabel;
    }

    private StackPanel CreateActions()
    {
        Button rejectButton = CreateActionButton(nameof(Strings.FriendsReject), ButtonLookCatalog.Secondary);
        rejectButton.Click += HandleRejectClick;
        Button acceptButton = CreateActionButton(nameof(Strings.FriendsAccept), ButtonLookCatalog.Primary);
        acceptButton.Click += HandleAcceptClick;

        var actionsStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = ButtonSpacing,
        };
        actionsStackPanel.AddChild(rejectButton);
        actionsStackPanel.AddChild(acceptButton);
        actionsStackPanel.Anchor(Gum.Wireframe.Anchor.Right);
        actionsStackPanel.X = -PaddingX;
        actionsStackPanel.SetBinding(nameof(IsVisible), nameof(FriendRequestsViewModel.AreToastActionsVisible));
        return actionsStackPanel;
    }

    private static Button CreateActionButton(string textKey, ButtonLook buttonLook)
    {
        Button button = ViewFactory.CreateButton(textKey, ActionButtonWidth, ActionButtonHeight);
        ButtonSkin.CreateAndAttach(button, buttonLook);
        return button;
    }

    private void HandleRejectClick(object? sender, EventArgs e)
    {
        _ = RunSafely(_requests.RejectToast());
    }

    private static async Task RunSafely(Task task)
    {
        try
        {
            await task;
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("The friend request toast could not respond because the connection is closed.", ex);
        }
    }

    private void HandleAcceptClick(object? sender, EventArgs e)
    {
        _ = RunSafely(_requests.AcceptToast());
    }
}
