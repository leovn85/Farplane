using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Farplane.Common;
using Farplane.Common.Dialogs;
using Farplane.FFX.Data;
using Farplane.FFX.Values;
using Farplane.Memory;
using MahApps.Metro.Controls;

namespace Farplane.FFX.EditorPanels.EquipmentPanel;

/// <summary>
///     Interaction logic for EquipmentPanel.xaml
/// </summary>
public partial class EquipmentPanel : UserControl
{
    const string NameUnknown = "????";
    const string NameEmpty = "< Empty >";

    readonly BitmapImage[] _icons =
    [
        new BitmapImage(new Uri("pack://application:,,,/FFX/Resources/MenuIcons/equip_0_0.png")),
        new BitmapImage(new Uri("pack://application:,,,/FFX/Resources/MenuIcons/equip_0_1.png")),
        new BitmapImage(new Uri("pack://application:,,,/FFX/Resources/MenuIcons/equip_1_0.png")),
        new BitmapImage(new Uri("pack://application:,,,/FFX/Resources/MenuIcons/equip_1_1.png")),
        new BitmapImage(new Uri("pack://application:,,,/FFX/Resources/MenuIcons/equip_2_0.png")),
        new BitmapImage(new Uri("pack://application:,,,/FFX/Resources/MenuIcons/equip_2_1.png")),
        new BitmapImage(new Uri("pack://application:,,,/FFX/Resources/MenuIcons/equip_3_0.png")),
        new BitmapImage(new Uri("pack://application:,,,/FFX/Resources/MenuIcons/equip_3_1.png")),
        new BitmapImage(new Uri("pack://application:,,,/FFX/Resources/MenuIcons/equip_4_0.png")),
        new BitmapImage(new Uri("pack://application:,,,/FFX/Resources/MenuIcons/equip_4_1.png")),
        new BitmapImage(new Uri("pack://application:,,,/FFX/Resources/MenuIcons/equip_5_0.png")),
        new BitmapImage(new Uri("pack://application:,,,/FFX/Resources/MenuIcons/equip_5_1.png")),
        new BitmapImage(new Uri("pack://application:,,,/FFX/Resources/MenuIcons/equip_6_0.png")),
        new BitmapImage(new Uri("pack://application:,,,/FFX/Resources/MenuIcons/equip_6_1.png")),
    ];

    EquipmentItem _currentItem;
    bool _refreshing;
    int _selectedItem = -1;

    public EquipmentPanel()
    {
        this.InitializeComponent();

        // Initialize equipment item view
        for (var charaIndex = 0; charaIndex < 18; charaIndex++)
        {
            this.ComboEquipmentCharacter.Items.Add((Character)charaIndex);
        }

        for (
            var formulaIndex = 0;
            formulaIndex < DamageFormula.DamageFormulas.Length;
            formulaIndex++
        )
        {
            this.ComboDamageFormula.Items.Add(DamageFormula.DamageFormulas[formulaIndex].Name);
        }
    }

    public void Refresh()
    {
        this._refreshing = true;

        this.ListEquipment.Items.Clear();

        var items = Equipment.ReadItems();

        for (var equipmentSlot = 0; equipmentSlot < Equipment.MaxItems; equipmentSlot++)
        {
            var imageIcon = new Image
            {
                Width = 24,
                Height = 24,
                Name = "ImageIcon",
            };
            var textName = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                Name = "TextName",
            };

            var panelItem = new DockPanel
            {
                Children = { imageIcon, textName },
                Margin = new Thickness(0),
            };
            var listItem = new ListViewItem { Content = panelItem };

            var currentItem = items[equipmentSlot];

            if (currentItem.Character > 6)
            {
                // Aeon or Seymour, no name available
                var charaName = ((Character)currentItem.Character).ToString();
                textName.Text = $"{NameUnknown} [{charaName}]";
            }
            else if (currentItem.SlotOccupied == 0)
            {
                // Empty slot
                textName.Text = NameEmpty;
            }
            else
            {
                textName.Text = EquipName.EquipNames[currentItem.Character][currentItem.Name];
                imageIcon.Source = this._icons[(currentItem.Character * 2) + currentItem.Type];
            }

            this.ListEquipment.Items.Add(listItem);
        }

        if (this.ListEquipment.SelectedIndex == -1)
        {
            this.ListEquipment.SelectedIndex = 0;
        }

        this.RefreshSelectedItem();

        this._refreshing = false;
    }

    void RefreshSelectedItem()
    {
        this._refreshing = true;

        if (this._selectedItem == -1)
        {
            this._selectedItem = 0;
        }

        this._currentItem = Equipment.ReadItem(this._selectedItem);

        if (
            (this._selectedItem >= 0x0C && this._selectedItem <= 0x21)
            || this._currentItem.SlotOccupied == 0
        )
        {
            this.ButtonDeleteItem.IsEnabled = false;
        }
        else
        {
            this.ButtonDeleteItem.IsEnabled = true;
        }

        var listItem = this.ListEquipment.Items[this._selectedItem] as ListViewItem;
        var imageIcon = (listItem.Content as DockPanel).FindChild<Image>("ImageIcon");
        var listText = (listItem.Content as DockPanel).FindChild<TextBlock>("TextName");

        if (this._currentItem.SlotOccupied == 0)
        {
            this.ContentNoItem.Visibility = Visibility.Visible;
            this.ContentEditItem.Visibility = Visibility.Collapsed;

            listText.Text = NameEmpty;
            imageIcon.Source = null;
            this._refreshing = false;
            return;
        }

        if (this._currentItem.Character < 7)
        {
            var nameString = EquipName.EquipNames[this._currentItem.Character][
                this._currentItem.Name
            ];

            listText.Text = nameString;
            imageIcon.Source = this._icons[
                (this._currentItem.Character * 2) + this._currentItem.Type
            ];

            this.ButtonEquipmentName.Content = nameString;
            this.GroupEquipmentEditor.Header = nameString;
        }
        else
        {
            var nameString = ((Character)this._currentItem.Character).ToString();

            listText.Text = $"{NameUnknown} [{nameString}]";
            imageIcon.Source = null;

            this.ButtonEquipmentName.Content = NameUnknown;
            this.GroupEquipmentEditor.Header = NameUnknown;
        }

        var appearance = EquipAppearance.EquipAppearances.FirstOrDefault(e =>
            e.ID == this._currentItem.Appearance
        );
        if (appearance == null)
        {
            this.ButtonEquipmentAppearance.Content = NameUnknown;
        }
        else
        {
            this.ButtonEquipmentAppearance.Content = appearance.Name;
        }

        this.ComboEquipmentCharacter.SelectedIndex = this._currentItem.Character;
        this.ComboEquipmentType.SelectedIndex = this._currentItem.Type;

        var damageFormula = DamageFormula.DamageFormulas.FirstOrDefault(f =>
            f.ID == this._currentItem.DamageFormula
        );

        if (damageFormula == null)
        {
            this.ComboDamageFormula.SelectedIndex = 0;
        }
        else
        {
            this.ComboDamageFormula.SelectedIndex = Array.IndexOf(
                DamageFormula.DamageFormulas,
                damageFormula
            );
        }

        this.TextAttackPower.Text = this._currentItem.AttackPower.ToString();
        this.TextCritChance.Text = this._currentItem.Critical.ToString();

        this.ComboEquipmentSlots.SelectedIndex = this._currentItem.AbilityCount;

        for (var slot = 0; slot < 4; slot++)
        {
            var button = (Button)this.FindName("Ability" + slot.ToString().Trim());

            if (slot >= this._currentItem.AbilityCount)
            {
                button.Visibility = Visibility.Collapsed;
                continue;
            }
            button.Visibility = Visibility.Visible;
            var ability = AutoAbility.FromID(this._currentItem.Abilities[slot]);

            if (ability == null)
            {
                // no ability in this slot
                button.Content = NameEmpty;
                continue;
            }

            button.Content = ability.Name;
        }

        this.ContentNoItem.Visibility = Visibility.Collapsed;
        this.ContentEditItem.Visibility = Visibility.Visible;

        this._refreshing = false;
    }

    void SelectedEquipment_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (this._refreshing)
        {
            return;
        }

        this._selectedItem = this.ListEquipment.SelectedIndex;
        this.RefreshSelectedItem();
    }

    void ComboEquipmentSlots_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (this._refreshing)
        {
            return;
        }

        var newSlots = (sender as ComboBox).SelectedIndex;
        for (var i = 0; i < 4; i++)
        {
            if (this.AbilityPanel.Children[i] is not Button abilityButton)
            {
                continue;
            }

            if (i >= newSlots)
            {
                abilityButton.Visibility = Visibility.Collapsed;
                this.WriteAbility(this._selectedItem, i, 0xFF);
            }
            else
            {
                abilityButton.Visibility = Visibility.Visible;
            }
        }

        var offset =
            Equipment.Offset
            + (this._selectedItem * Equipment.BlockLength)
            + (int)Marshal.OffsetOf<EquipmentItem>(nameof(EquipmentItem.AbilityCount));
        GameMemory.Write(offset, (byte)newSlots, false);

        this.RefreshSelectedItem();
    }

    void WriteAbility(int itemSlot, int abilitySlot, ushort abilityId)
    {
        this._currentItem.Abilities[abilitySlot] = abilityId;
        Equipment.WriteItem(itemSlot, this._currentItem);
    }

    void AbilityButton_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var index = int.Parse(button.Name.Substring(7));

        // Generate search list
        var searchList = new List<string>();
        foreach (var ability in AutoAbility.AutoAbilities)
        {
            searchList.Add($"{ability.ID:X4} {ability.Name}");
        }

        var searchDialog = new SearchDialog(searchList) { Owner = this.TryFindParent<Window>() };
        var searchComplete = searchDialog.ShowDialog();

        if (!searchComplete.HasValue || !searchComplete.Value)
        {
            return;
        }

        if (searchDialog.ResultIndex == -1)
        {
            // Write empty slot
            this.WriteAbility(this._selectedItem, index, 0xFF);
        }
        else
        {
            var ability = AutoAbility.AutoAbilities[searchDialog.ResultIndex];
            this.WriteAbility(this._selectedItem, index, (ushort)ability.ID);
        }

        this.RefreshSelectedItem();
    }

    void ButtonEquipmentName_OnClick(object sender, RoutedEventArgs e)
    {
        var currentChara = this._currentItem.Character;

        if (currentChara > 6)
        {
            currentChara = 0;
        }

        var searchList = new List<string>();
        for (var n = 0; n < EquipName.EquipNames[currentChara].Length; n++)
        {
            searchList.Add($"{n:X2} {EquipName.EquipNames[currentChara][n]}");
        }

        var currentName = this._currentItem.Name;

        var nameString = string.Empty;

        if (currentChara < 7)
        {
            nameString = EquipName.EquipNames[currentChara][currentName];
        }

        var searchDialog = new SearchDialog(searchList, nameString, false);
        var searchComplete = searchDialog.ShowDialog();

        if (!searchComplete.Value)
        {
            return;
        }

        var searchIndex = searchDialog.ResultIndex;

        var offset = Equipment.Offset + (this._selectedItem * Equipment.BlockLength);
        GameMemory.Write(offset, (byte)searchIndex, false);

        this.RefreshSelectedItem();
    }

    void ComboEquipmentCharacter_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (this._refreshing)
        {
            return;
        }

        var offset =
            Equipment.Offset
            + (this._selectedItem * Equipment.BlockLength)
            + (int)Marshal.OffsetOf<EquipmentItem>(nameof(EquipmentItem.Character));
        GameMemory.Write(offset, (byte)this.ComboEquipmentCharacter.SelectedIndex, false);

        this.RefreshSelectedItem();
    }

    void ComboEquipmentType_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (this._refreshing)
        {
            return;
        }

        var offset =
            Equipment.Offset
            + (this._selectedItem * Equipment.BlockLength)
            + (int)Marshal.OffsetOf<EquipmentItem>(nameof(EquipmentItem.Type));
        GameMemory.Write(offset, (byte)this.ComboEquipmentType.SelectedIndex, false);

        this.RefreshSelectedItem();
    }

    void ButtonEquipmentAppearance_OnClick(object sender, RoutedEventArgs e)
    {
        var currentChara = this._currentItem.Character;

        if (currentChara > 6)
        {
            currentChara = 0;
        }

        var searchList = new List<string>();
        for (var n = 0; n < EquipAppearance.EquipAppearances.Length; n++)
        {
            searchList.Add(
                $"{EquipAppearance.EquipAppearances[n].ID:X2} {EquipAppearance.EquipAppearances[n].Name}"
            );
        }

        var currentAppearance = this._currentItem.Appearance;

        var searchDialog = new SearchDialog(searchList, currentAppearance.ToString("X4"), false);
        var searchComplete = searchDialog.ShowDialog();

        if (!searchComplete.Value)
        {
            return;
        }

        var searchIndex = searchDialog.ResultIndex;
        var searchItem = EquipAppearance.EquipAppearances[searchIndex];

        var offset =
            Equipment.Offset
            + (this._selectedItem * Equipment.BlockLength)
            + (int)Marshal.OffsetOf<EquipmentItem>(nameof(EquipmentItem.Appearance));
        GameMemory.Write(offset, (ushort)searchItem.ID, false);

        this.RefreshSelectedItem();
    }

    void ButtonDeleteItem_Click(object sender, RoutedEventArgs e)
    {
        if (this._selectedItem is >= 0x0C and <= 0x21)
        {
            return;
        }

        var nameString = string.Empty;

        try
        {
            nameString = EquipName.EquipNames[this._currentItem.Character][this._currentItem.Name];
        }
        catch
        {
            nameString = "This item";
        }

        var confirm = MessageBox.Show(
            $"{nameString} will be permanently deleted!\n\nAre you sure?",
            "Confirm item deletion",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning
        );
        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        var itemEmpty = new EquipmentItem { Abilities = new ushort[4] };
        Equipment.WriteItem(this._selectedItem, itemEmpty);
        this.RefreshSelectedItem();
    }

    void ButtonCreateNew_Click(object sender, RoutedEventArgs e)
    {
        var itemEmpty = new EquipmentItem
        {
            SlotOccupied = 0x01,
            Appearance = 0x4002,
            Name = 0x11,
            Abilities = [0xFF, 0xFF, 0xFF, 0xFF],
            AbilityCount = 0x04,
            DamageFormula = 0x01,
            AttackPower = 15,
            Critical = 3,
            EquippedBy = 0xFF,
        };
        Equipment.WriteItem(this._selectedItem, itemEmpty);
        this.RefreshSelectedItem();
    }

    void ComboDamageFormula_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (this._refreshing)
        {
            return;
        }

        var formula = DamageFormula.DamageFormulas[this.ComboDamageFormula.SelectedIndex];
        this._currentItem.DamageFormula = (byte)formula.ID;
        Equipment.WriteItem(this._selectedItem, this._currentItem);
        this.RefreshSelectedItem();
    }

    void TextAttackPower_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (this._refreshing || e.Key != Key.Enter)
        {
            return;
        }

        try
        {
            this._currentItem.AttackPower = byte.Parse(this.TextAttackPower.Text);
            Equipment.WriteItem(this._selectedItem, this._currentItem);
        }
        catch
        {
            Error.Show("Please enter a number between 0 and 255.");
        }
    }

    void TextCritChance_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (this._refreshing || e.Key != Key.Enter)
        {
            return;
        }

        try
        {
            this._currentItem.Critical = byte.Parse(this.TextCritChance.Text);
            Equipment.WriteItem(this._selectedItem, this._currentItem);
        }
        catch
        {
            Error.Show("Please enter a number between 0 and 255.");
        }
    }
}
