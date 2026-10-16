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

        for (var charaIndex = 0; charaIndex < 18; charaIndex++)
        {
            this.ComboEquipmentCharacter.Items.Add((Character)charaIndex);
        }

        for (var formulaIndex = 0; formulaIndex < DamageFormula.DamageFormulas.Length; formulaIndex++)
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
                var charaName = ((Character)currentItem.Character).ToString();
                textName.Text = $"{NameUnknown} [{charaName}]";
            }
            else if (currentItem.SlotOccupied == 0)
            {
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

        var isProtectedSlot = this._selectedItem < 0x22 || this._currentItem.SlotOccupied == 0;
        this.ButtonDeleteItem.IsEnabled = !isProtectedSlot;
        this.ButtonSellItem.IsEnabled = !isProtectedSlot;

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
            var nameString = EquipName.EquipNames[this._currentItem.Character][this._currentItem.Name];
            listText.Text = nameString;
            imageIcon.Source = this._icons[(this._currentItem.Character * 2) + this._currentItem.Type];
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

        var appearance = EquipAppearance.EquipAppearances.FirstOrDefault(e => e.ID == this._currentItem.Appearance);
        this.ButtonEquipmentAppearance.Content = appearance == null ? NameUnknown : appearance.Name;

        // Display live calculated sell price
        var sellPrice = Equipment.CalculateSellPrice(this._currentItem);
        this.TextSellPrice.Text = $"{sellPrice:N0} Gil";

        // Display equipment flags
        this.CheckFlagCelestial.IsChecked = (this._currentItem.Flags & 0x04) != 0;
        this.CheckFlagBrotherhood.IsChecked = (this._currentItem.Flags & 0x08) != 0;
        this.CheckFlagSummon.IsChecked = (this._currentItem.Flags & 0x01) != 0;
        this.CheckFlagHidden.IsChecked = (this._currentItem.Flags & 0x02) != 0;

        this.ComboEquipmentCharacter.SelectedIndex = this._currentItem.Character;
        this.ComboEquipmentType.SelectedIndex = this._currentItem.Type;

        var damageFormula = DamageFormula.DamageFormulas.FirstOrDefault(f => f.ID == this._currentItem.DamageFormula);
        this.ComboDamageFormula.SelectedIndex = damageFormula == null
            ? 0
            : Array.IndexOf(DamageFormula.DamageFormulas, damageFormula);

        this.TextAttackPower.Text = this._currentItem.AttackPower.ToString();
        this.TextCritChance.Text = this._currentItem.Critical.ToString();
        this.ComboEquipmentSlots.SelectedIndex = Math.Min((int)this._currentItem.AbilityCount, 4);

        for (var slot = 0; slot < 4; slot++)
        {
            var button = (Button)this.FindName("Ability" + slot.ToString().Trim());
            if (slot >= this._currentItem.AbilityCount)
            {
                button.Visibility = Visibility.Collapsed;
                continue;
            }

            button.Visibility = Visibility.Visible;
            var raw = this._currentItem.Abilities != null && this._currentItem.Abilities.Length > slot
                ? this._currentItem.Abilities[slot]
                : (ushort)0xFF;

            var ability = AutoAbility.FromID(raw);
            button.Content = ability == null ? NameEmpty : ability.Name;
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

    void FlagCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (this._refreshing)
        {
            return;
        }

        byte flags = 0;
        if (this.CheckFlagSummon.IsChecked == true)
            flags |= 0x01;
        if (this.CheckFlagHidden.IsChecked == true)
            flags |= 0x02;
        if (this.CheckFlagCelestial.IsChecked == true)
            flags |= 0x04;
        if (this.CheckFlagBrotherhood.IsChecked == true)
            flags |= 0x08;

        this._currentItem.Flags = flags;
        Equipment.WriteItem(this._selectedItem, this._currentItem);
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

        this._currentItem.AbilityCount = (byte)newSlots;
        Equipment.WriteItem(this._selectedItem, this._currentItem);
        this.RefreshSelectedItem();
    }

    void WriteAbility(int itemSlot, int abilitySlot, ushort abilityId)
    {
        if (this._currentItem.Abilities == null || this._currentItem.Abilities.Length < 4)
        {
            this._currentItem.Abilities = new ushort[4];
        }
        this._currentItem.Abilities[abilitySlot] = abilityId;
        Equipment.WriteItem(itemSlot, this._currentItem);
        Party.RecalculateStats();
    }

    void AbilityButton_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var index = int.Parse(button.Name.Substring(7));

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
        if (currentChara < 7 && currentName < EquipName.EquipNames[currentChara].Length)
        {
            nameString = EquipName.EquipNames[currentChara][currentName];
        }

        var searchDialog = new SearchDialog(searchList, nameString, false);
        var searchComplete = searchDialog.ShowDialog();
        if (!searchComplete.HasValue || !searchComplete.Value)
        {
            return;
        }

        this._currentItem.Name = (byte)searchDialog.ResultIndex;
        Equipment.WriteItem(this._selectedItem, this._currentItem);
        this.RefreshSelectedItem();
    }

    void ComboEquipmentCharacter_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (this._refreshing)
        {
            return;
        }
        this._currentItem.Character = (byte)this.ComboEquipmentCharacter.SelectedIndex;
        Equipment.WriteItem(this._selectedItem, this._currentItem);
        Party.RecalculateStats();
        this.RefreshSelectedItem();
    }

    void ComboEquipmentType_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (this._refreshing)
        {
            return;
        }
        this._currentItem.Type = (byte)this.ComboEquipmentType.SelectedIndex;
        Equipment.WriteItem(this._selectedItem, this._currentItem);
        this.RefreshSelectedItem();
    }

    void ButtonEquipmentAppearance_OnClick(object sender, RoutedEventArgs e)
    {
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
        if (!searchComplete.HasValue || !searchComplete.Value)
        {
            return;
        }

        var searchItem = EquipAppearance.EquipAppearances[searchDialog.ResultIndex];
        this._currentItem.Appearance = (ushort)searchItem.ID;
        Equipment.WriteItem(this._selectedItem, this._currentItem);
        this.RefreshSelectedItem();
    }

    void ButtonDeleteItem_Click(object sender, RoutedEventArgs e)
    {
        if (this._selectedItem < 0x22)
        {
            Error.Show("Reserved character and story equipment slots (0 - 33) cannot be deleted.");
            return;
        }

        var nameString = this._currentItem.Character < 7 && this._currentItem.Name < EquipName.EquipNames[this._currentItem.Character].Length
            ? EquipName.EquipNames[this._currentItem.Character][this._currentItem.Name]
            : "This item";

        var confirm = MessageBox.Show(
            $"{nameString} will be permanently deleted!\n\nAre you sure?",
            "Confirm Deletion",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning
        );

        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        var itemEmpty = new EquipmentItem { Abilities = new ushort[4] };
        Equipment.WriteItem(this._selectedItem, itemEmpty);
        this.Refresh();
    }

    void ButtonSellItem_Click(object sender, RoutedEventArgs e)
    {
        if (!GameMemory.IsAttached)
        {
            return;
        }

        if (this._selectedItem < 0x22)
        {
            Error.Show("Reserved character and story equipment slots (0 - 33) cannot be sold.");
            return;
        }

        if (this._currentItem.SlotOccupied == 0)
        {
            return;
        }

        var equippedSlots = Equipment.GetEquippedSlots();
        if (equippedSlots.Contains(this._selectedItem) || (this._currentItem.EquippedBy & 0xFF) != 0xFF)
        {
            Error.Show("This equipment is currently equipped by a character or aeon and cannot be sold.");
            return;
        }

        if ((this._currentItem.Flags & 0x0F) != 0)
        {
            Error.Show("Celestial, Brotherhood, Summon, or Hidden equipment cannot be sold.");
            return;
        }

        var price = Equipment.CalculateSellPrice(this._currentItem);
        var nameString = this._currentItem.Character < 7 && this._currentItem.Name < EquipName.EquipNames[this._currentItem.Character].Length
            ? EquipName.EquipNames[this._currentItem.Character][this._currentItem.Name]
            : "This item";

        var confirm = MessageBox.Show(
            $"Sell {nameString} for {price:N0} Gil?",
            "Confirm Sale",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question
        );

        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        var newGil = Math.Min((long)General.CurrentGil + price, 999999999L);
        General.CurrentGil = (int)newGil;

        var emptyItem = new EquipmentItem { Abilities = new ushort[4] };
        Equipment.WriteItem(this._selectedItem, emptyItem);

        this.Refresh();
    }

    void ButtonBulkSell_Click(object sender, RoutedEventArgs e)
    {
        if (!GameMemory.IsAttached)
        {
            return;
        }

        var preserve4Slots = this.CheckPreserve4Slots.IsChecked == true;
        var preserveRares = this.CheckPreserveRareAbilities.IsChecked == true;

        var equippedSlots = Equipment.GetEquippedSlots();
        var allItems = Equipment.ReadItems();

        var candidates = new List<int>();
        var totalGain = 0L;

        for (var i = 0x22; i < Equipment.MaxItems; i++)
        {
            var item = allItems[i];
            if (item.SlotOccupied == 0)
            {
                continue;
            }

            // Must not be equipped
            if (equippedSlots.Contains(i) || (item.EquippedBy & 0xFF) != 0xFF)
            {
                continue;
            }

            // Must not have special flags (Celestial, Brotherhood, Summon, Hidden)
            if ((item.Flags & 0x0F) != 0)
            {
                continue;
            }

            // Optional filter: Keep 4-slot gear
            if (preserve4Slots && item.AbilityCount == 4)
            {
                continue;
            }

            // Optional filter: Keep rare abilities
            if (preserveRares && Equipment.HasRareAbilities(item))
            {
                continue;
            }

            candidates.Add(i);
            totalGain += Equipment.CalculateSellPrice(item);
        }

        if (candidates.Count == 0)
        {
            MessageBox.Show(
                "No eligible un-equipped equipment found for bulk sale.\n(Equipped, Celestial, story, and filtered items are always preserved).",
                "Bulk Sell",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
            return;
        }

        var prompt = MessageBox.Show(
            $"Found {candidates.Count} un-equipped items matching safety criteria.\n"
                + $"Total estimated proceeds: {totalGain:N0} Gil.\n\n"
                + "Do you want to sell these items now?",
            "Confirm Bulk Sell",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question
        );

        if (prompt != MessageBoxResult.Yes)
        {
            return;
        }

        var emptyItem = new EquipmentItem { Abilities = new ushort[4] };
        foreach (var slot in candidates)
        {
            Equipment.WriteItem(slot, emptyItem);
        }

        var finalGil = Math.Min((long)General.CurrentGil + totalGain, 999999999L);
        General.CurrentGil = (int)finalGil;

        this.Refresh();
        MessageBox.Show(
            $"Successfully sold {candidates.Count} items for {totalGain:N0} Gil!\nNew Gil balance: {General.CurrentGil:N0}",
            "Bulk Sell Completed",
            MessageBoxButton.OK,
            MessageBoxImage.Information
        );
    }

    void ButtonCreateNew_Click(object sender, RoutedEventArgs e)
    {
        var itemNew = new EquipmentItem
        {
            SlotOccupied = 0x01,
            Flags = 0x00,
            Appearance = 0x4002,
            Name = 0x11,
            Abilities = [0xFF, 0xFF, 0xFF, 0xFF],
            AbilityCount = 0x04,
            DamageFormula = 0x01,
            AttackPower = 15,
            Critical = 3,
            EquippedBy = 0xFF,
        };
        Equipment.WriteItem(this._selectedItem, itemNew);
        this.Refresh();
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
            this.TextAttackPower.SelectAll();
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
            this.TextCritChance.SelectAll();
        }
        catch
        {
            Error.Show("Please enter a number between 0 and 255.");
        }
    }
}
