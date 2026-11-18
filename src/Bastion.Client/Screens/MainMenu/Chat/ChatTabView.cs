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

        var chatChannelListView = new ChatChannelListView(chatTab);
        chatChannelListView.SetBinding(nameof(IsVisible), nameof(ChatTabViewModel.IsListVisible));
        var lobbyChatView = new LobbyChatView(chatTab.LobbyChat);
        var privateConversationView = new PrivateConversationView(chatTab.PrivateConversation);
        Panel globalPanePanel = ViewFactory.CreateToggledContainer(
            lobbyChatView,
            nameof(ChatTabViewModel.IsGlobalOpen));
        Panel privatePanePanel = ViewFactory.CreateToggledContainer(
            privateConversationView,
            nameof(ChatTabViewModel.IsPrivateOpen));
        AddChild(chatChannelListView);
        AddChild(globalPanePanel);
        AddChild(privatePanePanel);
    }
}
