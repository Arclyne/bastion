using System;
using Gum.DataTypes;
using Gum.Forms.Controls;

namespace Bastion.Client.Screens.MainMenu.Chat;

public sealed class ChatTabView : Panel
{
    public ChatTabView(ChatTabViewModel tab)
    {
        ArgumentNullException.ThrowIfNull(tab);

        BindingContext = tab;
        WidthUnits = DimensionUnitType.RelativeToChildren;
        HeightUnits = DimensionUnitType.RelativeToChildren;

        var channels = new ChatChannelListView(tab);
        channels.SetBinding(nameof(IsVisible), nameof(ChatTabViewModel.IsListVisible));
        AddChild(channels);
        AddChild(CreateGlobal(tab));
        AddChild(CreatePrivate(tab));
    }

    private static Panel CreateGlobal(ChatTabViewModel tab)
    {
        var container = new Panel
        {
            WidthUnits = DimensionUnitType.RelativeToChildren,
            HeightUnits = DimensionUnitType.RelativeToChildren,
        };
        var conversation = new ChatConversationView(tab.Lobby) { BindingContext = tab.Lobby };
        container.AddChild(conversation);
        container.SetBinding(nameof(IsVisible), nameof(ChatTabViewModel.IsGlobalOpen));
        return container;
    }

    private static Panel CreatePrivate(ChatTabViewModel tab)
    {
        var container = new Panel
        {
            WidthUnits = DimensionUnitType.RelativeToChildren,
            HeightUnits = DimensionUnitType.RelativeToChildren,
        };
        container.AddChild(new PrivateConversationView(tab.Private));
        container.SetBinding(nameof(IsVisible), nameof(ChatTabViewModel.IsPrivateOpen));
        return container;
    }
}
