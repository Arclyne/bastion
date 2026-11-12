using System;
using Bastion.Client.Localization;
using Bastion.Client.Theme;
using Gum.DataTypes;
using Gum.Forms.Controls;

namespace Bastion.Client.Controls;

public sealed class LanguagePicker : ComboBox
{
    public LanguagePicker(ILanguageService languages, float width, float height)
    {
        ArgumentNullException.ThrowIfNull(languages);

        Width = width;
        WidthUnits = DimensionUnitType.Absolute;
        Height = height;
        HeightUnits = DimensionUnitType.Absolute;
        DisplayMemberPath = nameof(SupportedLanguage.DisplayName);
        PickerSkin.CreateAndAttach(this);
        languages.LanguageChanged += HandleLanguageChanged;
    }

    private void HandleLanguageChanged(object? sender, EventArgs e)
    {
        RefreshListItems();
        UpdateToObject(SelectedObject);
    }

    private void RefreshListItems()
    {
        if (Items is null)
        {
            return;
        }

        int itemCount = Math.Min(ListBox.ListBoxItems.Count, Items.Count);
        for (int itemIndex = 0; itemIndex < itemCount; itemIndex++)
        {
            ListBox.ListBoxItems[itemIndex].UpdateToObject(Items[itemIndex]);
        }
    }
}
