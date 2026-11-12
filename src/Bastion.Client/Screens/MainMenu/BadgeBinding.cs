using System;

namespace Bastion.Client.Screens.MainMenu;

public sealed record BadgeBinding
{
    public BadgeBinding(string textProperty, string visibilityProperty)
    {
        ArgumentNullException.ThrowIfNull(textProperty);
        ArgumentNullException.ThrowIfNull(visibilityProperty);

        TextProperty = textProperty;
        VisibilityProperty = visibilityProperty;
    }

    public string TextProperty { get; }

    public string VisibilityProperty { get; }
}
