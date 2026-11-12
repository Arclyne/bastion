using System;
using Gum.DataTypes;
using Gum.Forms.Controls;

namespace Bastion.Client.Screens.MainMenu.Chat;

public sealed class ChatTabView : Panel
{
    public ChatTabView(ChatTabViewModel chatTab)
    {
        ArgumentNullException.ThrowIfNull(chatTab);

        BindingContext = chatTab;
        WidthUnits = DimensionUnitType.RelativeToChildren;
        HeightUnits = DimensionUnitType.RelativeToChildren;

        var channels = new ChatChannelListView(chatTab);
        channels.SetBinding(nameof(IsVisible), nameof(ChatTabViewModel.IsListVisible));
        var lobbyChat = new LobbyChatView(chatTab.LobbyChat);
        var privateConversation = new PrivateConversationView(chatTab.PrivateConversation);
        AddChild(channels);
        AddChild(CreateToggledContainer(lobbyChat, nameof(ChatTabViewModel.IsGlobalOpen)));
        AddChild(CreateToggledContainer(privateConversation, nameof(ChatTabViewModel.IsPrivateOpen)));
    }

    private static Panel CreateToggledContainer(FrameworkElement content, string visibilityProperty)
    {
        var container = new Panel
        {
            WidthUnits = DimensionUnitType.RelativeToChildren,
            HeightUnits = DimensionUnitType.RelativeToChildren,
        };
        container.AddChild(content);
        container.SetBinding(nameof(IsVisible), visibilityProperty);
        return container;
    }
}
