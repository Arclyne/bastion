using System;

namespace Bastion.Client.Screens.MainMenu.Rooms;

public sealed record NoticeBinding
{
    public NoticeBinding(string textProperty, string visibilityProperty)
    {
        ArgumentNullException.ThrowIfNull(textProperty);
        ArgumentNullException.ThrowIfNull(visibilityProperty);

        TextProperty = textProperty;
        VisibilityProperty = visibilityProperty;
    }

    public string TextProperty { get; }

    public string VisibilityProperty { get; }
}
