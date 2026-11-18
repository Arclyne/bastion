using System;
using System.Threading.Tasks;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;
using log4net;

namespace Bastion.Client.Screens.MainMenu.Friends;

public sealed class FriendRequestRowView : Panel
{
    private const float RowWidth = PanelMetrics.ContentWidth;
    private const float RowHeight = 56;
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
        StackPanel textStackPanel = CreateText();
        StackPanel actionsStackPanel = row.IsReceived ? CreateReceivedActions() : CreateSentActions();
        AddChild(textStackPanel);
        AddChild(actionsStackPanel);
    }

    private static StackPanel CreateText()
    {
        Label nicknameLabel = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(FriendRequestRowViewModel.Nickname),
            TextStyleCatalog.IdentityName);
        Label detailLabel = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(FriendRequestRowViewModel.DetailText),
            TextStyleCatalog.Hint);
        StackPanel textStackPanel = ViewFactory.CreateCenteredLines(nicknameLabel, detailLabel);
        textStackPanel.X = PanelMetrics.RowContentPaddingX;
        return textStackPanel;
    }

    private StackPanel CreateReceivedActions()
    {
        Button rejectButton = CreateActionButton(nameof(Strings.FriendsReject), ButtonLookCatalog.Secondary);
        rejectButton.Click += HandleRejectClick;
        Button acceptButton = CreateActionButton(nameof(Strings.FriendsAccept), ButtonLookCatalog.Primary);
        acceptButton.Click += HandleAcceptClick;
        return CreateActionRow(rejectButton, acceptButton);
    }

    private static Button CreateActionButton(string textKey, ButtonLook buttonLook)
    {
        Button button = ViewFactory.CreateButton(textKey, ActionButtonWidth, ActionButtonHeight);
        ButtonSkin.CreateAndAttach(button, buttonLook);
        button.SetBinding(nameof(IsEnabled), nameof(FriendRequestRowViewModel.CanAct));
        return button;
    }

    private void HandleRejectClick(object? sender, EventArgs e)
    {
        _ = RunSafely(_requests.Reject(_row));
    }

    private static async Task RunSafely(Task task)
    {
        try
        {
            await task;
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("The friend request action could not be sent because the connection is closed.", ex);
        }
    }

    private void HandleAcceptClick(object? sender, EventArgs e)
    {
        _ = RunSafely(_requests.Accept(_row));
    }

    private StackPanel CreateActionRow(params Button[] buttons)
    {
        var actionsStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = ButtonSpacing,
        };
        foreach (Button button in buttons)
        {
            actionsStackPanel.AddChild(button);
        }

        actionsStackPanel.Anchor(Gum.Wireframe.Anchor.Right);
        return actionsStackPanel;
    }

    private StackPanel CreateSentActions()
    {
        Button cancelButton = CreateActionButton(nameof(Strings.FriendsCancelRequest), ButtonLookCatalog.Secondary);
        cancelButton.Click += HandleCancelClick;
        return CreateActionRow(cancelButton);
    }

    private void HandleCancelClick(object? sender, EventArgs e)
    {
        _ = RunSafely(_requests.Cancel(_row));
    }
}
