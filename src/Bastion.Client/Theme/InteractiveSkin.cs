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

    public abstract void ShowState(InteractionStates flags);

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

        ShowState(InteractionStates.None);
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
            InteractionStates flags = ReadFlags(state.Name);
            var skinState = new SkinState(this, flags);
            state.Apply = skinState.Show;
        }
    }

    private static InteractionStates ReadFlags(string stateName)
    {
        InteractionStates flags = InteractionStates.None;
        flags |= ReadFlag(stateName, HoveredMarker, InteractionStates.Hovered);
        flags |= ReadFlag(stateName, PressedMarker, InteractionStates.Pressed | InteractionStates.Hovered);
        flags |= ReadFlag(stateName, FocusedMarker, InteractionStates.Focused);
        flags |= ReadFlag(stateName, DisabledMarker, InteractionStates.Disabled);
        if (stateName.EndsWith(CheckedSuffix, StringComparison.Ordinal))
        {
            flags |= InteractionStates.Checked;
        }

        return flags;
    }

    private static InteractionStates ReadFlag(string stateName, string marker, InteractionStates flag)
    {
        if (!stateName.Contains(marker, StringComparison.Ordinal))
        {
            return InteractionStates.None;
        }

        return flag;
    }
}
