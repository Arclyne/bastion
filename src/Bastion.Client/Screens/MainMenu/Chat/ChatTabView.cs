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
        Panel globalPane = ViewFactory.CreateToggledContainer(lobbyChat, nameof(ChatTabViewModel.IsGlobalOpen));
        Panel privatePane = ViewFactory.CreateToggledContainer(
            privateConversation,
            nameof(ChatTabViewModel.IsPrivateOpen));
        AddChild(channels);
        AddChild(globalPane);
        AddChild(privatePane);
    }
}
