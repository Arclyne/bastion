using System;
using Gum.DataTypes;
using Gum.Forms.Controls;

namespace Bastion.Client.Screens.MainMenu.Chat;

public sealed class LobbyChatView : Panel
{
    public LobbyChatView(LobbyChatViewModel chat)
    {
        ArgumentNullException.ThrowIfNull(chat);

        BindingContext = chat;
        WidthUnits = DimensionUnitType.RelativeToChildren;
        HeightUnits = DimensionUnitType.RelativeToChildren;

        var channels = new ChatChannelListView(chat);
        channels.SetBinding(nameof(IsVisible), nameof(LobbyChatViewModel.IsChannelListVisible));
        var conversation = new ChatConversationView(chat);
        conversation.SetBinding(nameof(IsVisible), nameof(LobbyChatViewModel.IsConversationOpen));
        AddChild(channels);
        AddChild(conversation);
    }
}
