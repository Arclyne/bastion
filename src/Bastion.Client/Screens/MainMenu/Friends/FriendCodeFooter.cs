using System;
using Bastion.Client.Theme;
using Gum.Forms.Controls;

namespace Bastion.Client.Screens.MainMenu.Friends;

public sealed class FriendCodeFooter : StackPanel
{
    private const float FooterSpacing = 12;
    private const float CopyButtonWidth = 96;
    private const float CopyButtonHeight = 36;

    private readonly FriendsPanelViewModel _friendsPanelViewModel;

    public FriendCodeFooter(FriendsPanelViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        _friendsPanelViewModel = viewModel;
        Orientation = Orientation.Horizontal;
        Spacing = FooterSpacing;
        Label codeLabel = CreateCodeLabel();
        Button copyButton = CreateCopyButton();
        AddChild(codeLabel);
        AddChild(copyButton);
    }

    private static Label CreateCodeLabel()
    {
        Label label = ViewFactory.CreateBoundUntranslatedLabel(nameof(FriendsPanelViewModel.OwnFriendCodeText), TextStyleCatalog.IdentityDetail);
        ViewFactory.CenterVertically(label);
        return label;
    }

    private Button CreateCopyButton()
    {
        Button button = ViewFactory.CreateBoundButton(
            nameof(FriendsPanelViewModel.CopyButtonKey),
            CopyButtonWidth,
            CopyButtonHeight);
        ButtonSkin.CreateAndAttach(button, ButtonLookCatalog.Secondary);
        button.Click += HandleCopyClick;
        return button;
    }

    private void HandleCopyClick(object? sender, EventArgs e)
    {
        _friendsPanelViewModel.CopyOwnCode();
    }
}
