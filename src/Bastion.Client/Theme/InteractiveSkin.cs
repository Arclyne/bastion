using System;
using Gum.DataTypes.Variables;
using Gum.Forms.Controls;

namespace Bastion.Client.Theme;

public abstract class InteractiveSkin
{
    private const string EnabledStatePrefix = "Enabled";
    private const string HoveredMarker = "Highlighted";
    private const string PressedMarker = "Pushed";
    private const string FocusedMarker = "Focused";
    private const string DisabledMarker = "Disabled";
    private const string CheckedSuffix = "On";

    public abstract void ShowState(InteractionFlags flags);

    protected void Attach(FrameworkElement control)
    {
        ArgumentNullException.ThrowIfNull(control);

        foreach (StateSaveCategory category in control.Visual.Categories.Values)
        {
            if (IsInteractionCategory(category))
            {
                ReplaceStates(category);
            }
        }

        ShowState(InteractionFlags.None);
    }

    private static bool IsInteractionCategory(StateSaveCategory category)
    {
        foreach (StateSave state in category.States)
        {
            if (state.Name.StartsWith(EnabledStatePrefix, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private void ReplaceStates(StateSaveCategory category)
    {
        foreach (StateSave state in category.States)
        {
            InteractionFlags flags = ReadFlags(state.Name);
            var skinState = new SkinState(this, flags);
            state.Apply = skinState.Show;
        }
    }

    private static InteractionFlags ReadFlags(string stateName)
    {
        InteractionFlags flags = InteractionFlags.None;
        flags |= ReadFlag(stateName, HoveredMarker, InteractionFlags.Hovered);
        flags |= ReadFlag(stateName, PressedMarker, InteractionFlags.Pressed | InteractionFlags.Hovered);
        flags |= ReadFlag(stateName, FocusedMarker, InteractionFlags.Focused);
        flags |= ReadFlag(stateName, DisabledMarker, InteractionFlags.Disabled);
        if (stateName.EndsWith(CheckedSuffix, StringComparison.Ordinal))
        {
            flags |= InteractionFlags.Checked;
        }

        return flags;
    }

    private static InteractionFlags ReadFlag(string stateName, string marker, InteractionFlags flag)
    {
        if (!stateName.Contains(marker, StringComparison.Ordinal))
        {
            return InteractionFlags.None;
        }

        return flag;
    }
}
