using System;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.Forms.Controls;
using log4net;

namespace Bastion.Client.Screens.MainMenu.Rooms;

public sealed class KickPlayerDialogView : DialogCard
{
    private static readonly ILog _logger = LogManager.GetLogger(typeof(KickPlayerDialogView));

    private readonly PrivateRoomViewModel _room;

    public KickPlayerDialogView(PrivateRoomViewModel room)
    {
        ArgumentNullException.ThrowIfNull(room);

        _room = room;
        BindingContext = room;
        Anchor(Gum.Wireframe.Anchor.Center);
        Label titleLabel = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(PrivateRoomViewModel.KickTitleText),
            TextStyleCatalog.DialogTitle,
            DialogMetrics.ContentWidth);
        Label messageLabel = ViewFactory.CreateLabel(nameof(Strings.RoomKickMessage), TextStyleCatalog.DialogMessage);
        ViewFactory.SetFixedWidth(messageLabel, DialogMetrics.ContentWidth);
        Content.AddChild(titleLabel);
        Content.AddChild(messageLabel);
        Content.AddChild(CreateButtons());
    }

    private StackPanel CreateButtons()
    {
        Button cancelButton = CreateDialogButton(nameof(Strings.RoomCancel), ButtonLookCatalog.Secondary);
        cancelButton.Click += HandleCancelClick;
        Button kickButton = CreateDialogButton(nameof(Strings.RoomKick), ButtonLookCatalog.Primary);
        kickButton.SetBinding(nameof(IsEnabled), nameof(PrivateRoomViewModel.CanSend));
        kickButton.Click += HandleKickClick;
        var rowStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = DialogMetrics.ButtonSpacing,
        };
        rowStackPanel.AddChild(cancelButton);
        rowStackPanel.AddChild(kickButton);
        return rowStackPanel;
    }

    private void HandleCancelClick(object? sender, EventArgs e)
    {
        _room.CancelKick();
    }

    private async void HandleKickClick(object? sender, EventArgs e)
    {
        try
        {
            await _room.ConfirmKick();
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("The player could not be kicked because the connection is closed.", ex);
        }
    }
}
