using System;
using Bastion.Client.Theme;
using Gum;
using Gum.Forms.Controls;
using Gum.Forms.DefaultVisuals.V3;
using log4net;

namespace Bastion.Client.Screens.MainMenu.Friends;

public sealed class FriendCodeFooter : StackPanel
{
    private const float FooterSpacing = 12;
    private const float CopyButtonWidth = 96;
    private const float CopyButtonHeight = 36;

    private static readonly ILog _logger = LogManager.GetLogger(typeof(FriendCodeFooter));

    private readonly FriendsPanelViewModel _viewModel;

    public FriendCodeFooter(FriendsPanelViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _viewModel = viewModel;
        Orientation = Orientation.Horizontal;
        Spacing = FooterSpacing;
        AddChild(CreateCodeLabel());
        AddChild(CreateCopyButton());
    }

    private static Label CreateCodeLabel()
    {
        Label label = ViewFactory.CreateLabel(string.Empty, TextStyles.IdentityDetail);
        ((LabelVisual)label.Visual).LocalizeText = false;
        label.SetBinding(nameof(Label.Text), nameof(FriendsPanelViewModel.OwnFriendCodeText));
        ViewFactory.CenterVertically(label);
        return label;
    }

    private Button CreateCopyButton()
    {
        Button button = ViewFactory.CreateButton(string.Empty, CopyButtonWidth, CopyButtonHeight);
        ButtonSkin.CreateAndAttach(button, ButtonLooks.Secondary);
        button.SetBinding(nameof(Button.Text), nameof(FriendsPanelViewModel.CopyButtonKey));
        button.Click += HandleCopyClick;
        return button;
    }

    private void HandleCopyClick(object? sender, EventArgs e)
    {
        if (GumService.Default.Clipboard is null)
        {
            _logger.Warn("The friend code could not be copied because there is no clipboard.");
            return;
        }

        GumService.Default.Clipboard.SetText(_viewModel.FormattedFriendCode);
        _viewModel.MarkCodeCopied();
    }
}
