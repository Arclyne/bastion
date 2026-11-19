using System;
using System.ComponentModel;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Bastion.Domain.Rooms;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;
using log4net;
using Microsoft.Xna.Framework;

namespace Bastion.Client.Screens.MainMenu.Rooms;

public sealed class JoinRoomDialogView : DialogCard
{
    private const float BoxSize = 52;
    private const float BoxSpacing = 8;
    private const float InputHeight = 63;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(JoinRoomDialogView));

    private readonly PrivateRoomViewModel _room;
    private readonly JoinRoomDialogViewModel _joinDialog;
    private readonly RoomCodeBoxes _codeBoxes;
    private readonly TextBox _codeTextBox;

    public JoinRoomDialogView(PrivateRoomViewModel room)
    {
        ArgumentNullException.ThrowIfNull(room);

        _room = room;
        _joinDialog = room.JoinDialog;
        BindingContext = room.JoinDialog;
        Anchor(Gum.Wireframe.Anchor.Center);
        _codeBoxes = new RoomCodeBoxes(BoxSize, TextStyleCatalog.JoinCodeLetter, BoxSpacing);
        _codeTextBox = CreateCodeTextBox();
        Label titleLabel = ViewFactory.CreateLabel(nameof(Strings.RoomJoinTitle), TextStyleCatalog.DialogTitle);
        Label messageLabel = ViewFactory.CreateLabel(nameof(Strings.RoomJoinMessage), TextStyleCatalog.DialogMessage);
        ViewFactory.SetFixedWidth(messageLabel, DialogMetrics.ContentWidth);
        Content.AddChild(titleLabel);
        Content.AddChild(messageLabel);
        Content.AddChild(CreateCodeInput());
        Content.AddChild(CreateHint());
        Content.AddChild(CreateError());
        Content.AddChild(CreateButtons());
        _joinDialog.PropertyChanged += HandleJoinDialogPropertyChanged;
    }

    private static TextBox CreateCodeTextBox()
    {
        var codeTextBox = new TextBox();
        codeTextBox.Dock(Gum.Wireframe.Dock.Fill);
        ShapeFactory.HideDefaultParts(codeTextBox);
        var textBoxVisual = (TextBoxBaseVisual)codeTextBox.Visual;
        textBoxVisual.ForegroundColor = Color.Transparent;
        textBoxVisual.CaretColor = Color.Transparent;
        textBoxVisual.PlaceholderColor = Color.Transparent;
        textBoxVisual.SelectionBackgroundColor = Color.Transparent;
        codeTextBox.SetBinding(nameof(TextBox.Text), nameof(JoinRoomDialogViewModel.CodeText));
        return codeTextBox;
    }

    private Panel CreateCodeInput()
    {
        var inputPanel = new Panel
        {
            Width = DialogMetrics.ContentWidth,
            WidthUnits = DimensionUnitType.Absolute,
            Height = InputHeight,
            HeightUnits = DimensionUnitType.Absolute,
        };
        ViewFactory.CenterHorizontally(_codeBoxes);
        inputPanel.AddChild(_codeBoxes);
        inputPanel.AddChild(_codeTextBox);
        return inputPanel;
    }

    private static Label CreateHint()
    {
        Label hintLabel = ViewFactory.CreateLabel(nameof(Strings.RoomJoinHint), TextStyleCatalog.Hint);
        hintLabel.SetBinding(nameof(IsVisible), nameof(JoinRoomDialogViewModel.IsHintShown));
        return hintLabel;
    }

    private static Label CreateError()
    {
        Label errorLabel = ViewFactory.CreateBoundUntranslatedLabel(
            nameof(JoinRoomDialogViewModel.ErrorText),
            TextStyleCatalog.FieldError,
            DialogMetrics.ContentWidth);
        errorLabel.SetBinding(nameof(IsVisible), nameof(JoinRoomDialogViewModel.HasError));
        return errorLabel;
    }

    private StackPanel CreateButtons()
    {
        Button cancelButton = CreateDialogButton(nameof(Strings.RoomCancel), ButtonLookCatalog.Secondary);
        cancelButton.Click += HandleCancelClick;
        Button enterButton = ViewFactory.CreateBoundButton(
            nameof(JoinRoomDialogViewModel.EnterKey),
            DialogMetrics.ButtonWidth,
            DialogMetrics.ButtonHeight);
        ButtonSkin.CreateAndAttach(enterButton, ButtonLookCatalog.Primary);
        enterButton.SetBinding(nameof(IsEnabled), nameof(JoinRoomDialogViewModel.CanEnter));
        enterButton.Click += HandleEnterClick;
        var rowStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = DialogMetrics.ButtonSpacing,
        };
        rowStackPanel.AddChild(cancelButton);
        rowStackPanel.AddChild(enterButton);
        return rowStackPanel;
    }

    private void HandleCancelClick(object? sender, EventArgs e)
    {
        _joinDialog.Close();
    }

    private async void HandleEnterClick(object? sender, EventArgs e)
    {
        try
        {
            await _room.JoinWithCode();
        }
        catch (ObjectDisposedException ex)
        {
            _logger.Warn("The room could not be joined because the connection is closed.", ex);
        }
    }

    private void HandleJoinDialogPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(JoinRoomDialogViewModel.CodeText))
        {
            ShowCode();
            return;
        }

        if (e.PropertyName == nameof(JoinRoomDialogViewModel.IsOpen) && _joinDialog.IsOpen)
        {
            ShowCode();
            _codeTextBox.IsFocused = true;
        }
    }

    private void ShowCode()
    {
        string code = _joinDialog.CodeText;
        _codeBoxes.ShowCode(code);
        _codeBoxes.HighlightBox(Math.Min(code.Length, RoomPolicy.CodeLength - 1));
    }
}
