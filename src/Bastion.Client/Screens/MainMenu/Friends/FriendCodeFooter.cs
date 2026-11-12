using System;
using Bastion.Client.Theme;
using Gum.Forms.Controls;

namespace Bastion.Client.Screens.MainMenu.Friends;

public sealed class FriendCodeFooter : StackPanel
{
    private const float FooterSpacing = 12;
    private const float CopyButtonWidth = 96;
    private const float CopyButtonHeight = 36;

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
        Label label = ViewFactory.CreateBoundUntranslatedLabel(nameof(FriendsPanelViewModel.OwnFriendCodeText), TextStyleCatalog.IdentityDetail);
        ViewFactory.CenterVertically(label);
        return label;
    }

    private Button CreateCopyButton()
    {
        Button button = ViewFactory.CreateButton(string.Empty, CopyButtonWidth, CopyButtonHeight);
        ButtonSkin.CreateAndAttach(button, ButtonLookCatalog.Secondary);
        button.SetBinding(nameof(Button.Text), nameof(FriendsPanelViewModel.CopyButtonKey));
        button.Click += HandleCopyClick;
        return button;
    }

    private void HandleCopyClick(object? sender, EventArgs e)
    {
        _viewModel.CopyOwnCode();
    }
}
