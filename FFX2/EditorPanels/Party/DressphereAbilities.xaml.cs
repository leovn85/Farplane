using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Farplane.FFX2.Values;

namespace Farplane.FFX2.EditorPanels;

/// <summary>
/// Interaction logic for DressphereAbilities.xaml
/// </summary>
public partial class DressphereAbilities : UserControl
{
    int _baseOffset = 0;
    readonly Button[] buttons = new Button[16];
    readonly int[] values = new int[16];

    int _selectedDs = 0;

    public int SelectedIndex { get; set; } = 0;

    public DressphereAbilities()
    {
        this.InitializeComponent();
        //Dressphere.Items.Add("None");
        //foreach (var dressphere in Dresspheres.GetDresspheres())
        //    Dressphere.Items.Add(dressphere.Name);
        //Dressphere.SelectedIndex = 0;
        this.ReloadDresspheres();
    }

    public void ReloadDresspheres()
    {
        var dresses = Dresspheres.GetDresspheres().ToList();
        dresses.RemoveAll(r => r.Special != -1 && r.Special != this.SelectedIndex);

        var oldSelection = this._selectedDs;
        var newSelection = 0;
        this.Dressphere.Items.Clear();
        this.Dressphere.Items.Add("None");
        foreach (var dress in dresses)
        {
            this.Dressphere.Items.Add(new ComboBoxItem { Content = dress.Name, Tag = dress.ID });
            if (dress.ID == oldSelection)
            {
                newSelection = this.Dressphere.Items.Count - 1;
                this._selectedDs = dress.ID;
            }
        }

        this.Dressphere.SelectedIndex = newSelection;
    }

    public void RefreshAbilities()
    {
        var selectedId = (int)((this.Dressphere.SelectedItem as ComboBoxItem)?.Tag ?? 0);

        if (selectedId == 0)
        {
            return;
        }

        var dressInfo = Dresspheres.GetDresspheres().FirstOrDefault(ds => ds.ID == selectedId);
        if (dressInfo == null)
        {
            return;
        }

        // Special dresspheres always fall under Yuna's offset
        this._baseOffset =
            (int)OffsetType.AbilityBase
            + (dressInfo.Special == -1 ? this.SelectedIndex * 0x6A0 : 0);

        var abilities = dressInfo.Abilities;

        for (var a = 0; a < 16; a++)
        {
            var button = (Button)this.FindName($"Ability{a}");
            if (button == null)
            {
                continue;
            }

            button.Content = string.Empty;
            button.IsEnabled = false;

            if (a >= abilities.Length)
            {
                continue;
            }

            var abil = abilities[a];

            var currentAP = 0;

            if (abil.Offset != -1)
            {
                currentAP = LegacyMemoryReader.ReadInt16(this._baseOffset + abil.Offset);
            }

            this.values[a] = currentAP;

            string apText;

            if (currentAP >= abil.MasteredAP || abil.MasteredAP == 0)
            {
                apText = " [***]";
            }
            else
            {
                apText = $" {currentAP} / {abil.MasteredAP}";
            }

            button.Content = $"{abil.Name} {apText}";

            if (!abil.ReadOnly)
            {
                button.IsEnabled = true;
            }
        }
    }

    void Dressphere_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        this._selectedDs = (int)((this.Dressphere.SelectedItem as ComboBoxItem)?.Tag ?? 0);
        this.RefreshAbilities();
    }

    void AbilityButton_Click(object sender, RoutedEventArgs e)
    {
        if (this.Dressphere.SelectedIndex == 0 || this._baseOffset == 0)
        {
            return;
        }

        var abilityNum = int.Parse((sender as Button).Name.Substring(7));
        var selectedId = (int)((this.Dressphere.SelectedItem as ComboBoxItem)?.Tag ?? this._selectedDs);
        var dressInfo = Dresspheres
            .GetDresspheres()
            .FirstOrDefault(ds => ds.ID == selectedId);
        if (dressInfo == null)
            return;
        var ability = dressInfo.Abilities[abilityNum];

        var apDialog = new AbilitySlider(ability.MasteredAP, this.values[abilityNum]);
        var setAp = apDialog.ShowDialog();

        if (!setAp.HasValue || !setAp.Value)
        {
            return;
        }

        var newAp = apDialog.AP;

        LegacyMemoryReader.WriteBytes(
            this._baseOffset + ability.Offset,
            BitConverter.GetBytes((ushort)newAp)
        );

        this.RefreshAbilities();
    }

    void MasterAll_Click(object sender, RoutedEventArgs e)
    {
        if (this.Dressphere.SelectedIndex == 0 || this._baseOffset == 0)
        {
            return;
        }
        var selectedId = (int)((this.Dressphere.SelectedItem as ComboBoxItem)?.Tag ?? this._selectedDs);
        var dressInfo = Dresspheres
            .GetDresspheres()
            .FirstOrDefault(ds => ds.ID == selectedId);
        if (dressInfo == null)
        {
            return;
        }

        var abilities = dressInfo.Abilities;

        for (var a = 0; a < 16; a++)
        {
            if (a >= abilities.Length)
            {
                continue;
            }

            var abil = abilities[a];

            if (abil.Offset == -1 || abil.ReadOnly)
            {
                continue;
            }

            LegacyMemoryReader.WriteBytes(
                this._baseOffset + abil.Offset,
                BitConverter.GetBytes((ushort)abil.MasteredAP)
            );
        }

        this.RefreshAbilities();
    }
}
