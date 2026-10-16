using System;
using System.Windows;
using System.Windows.Controls;
using Farplane.Common;
using Farplane.FFX.Data;
using Farplane.FFX.Values;

namespace Farplane.FFX.EditorPanels.SkillEditorPanel;

public partial class SkillDataPanel : UserControl
{
    int _currentIndex = -1;
    KernelType _currentType = KernelType.Command;
    KernelCommandData _currentData;

    public SkillDataPanel()
    {
        this.InitializeComponent();

        foreach (var formula in DamageFormula.DamageFormulas)
        {
            this.ComboDamageFormula.Items.Add(formula.Name);
        }

        this.ComboCharacterUser.Items.Add("Any / None (-1)");
        for (var c = 0; c < 18; c++)
        {
            this.ComboCharacterUser.Items.Add(((Character)c).ToString());
        }
    }

    public void LoadCommand(int index, KernelType type)
    {
        this._currentIndex = index;
        this._currentType = type;
        this._currentData = Skills.ReadData(index, type);

        this.TextCostMp.Text = this._currentData.CostMp.ToString();
        this.TextCostOverdrive.Text = this._currentData.CostOverdrive.ToString();
        this.TextMoveRank.Text = this._currentData.MoveRank.ToString();
        this.TextAttackPower.Text = this._currentData.AttackPower.ToString();
        this.TextAttackAccuracy.Text = this._currentData.AttackAccuracy.ToString();
        this.TextHitCount.Text = this._currentData.HitCount.ToString();
        this.TextCritBonus.Text = this._currentData.AttackCritBonus.ToString();
        this.TextShatterChance.Text = this._currentData.ShatterChance.ToString();

        // Formula
        var form = DamageFormula.FromID(this._currentData.DamageFormula);
        this.ComboDamageFormula.SelectedIndex = form != null ? Array.IndexOf(DamageFormula.DamageFormulas, form) : 0;

        // Character User
        this.ComboCharacterUser.SelectedIndex = this._currentData.CharacterUser is >= 0 and < 18
            ? this._currentData.CharacterUser + 1
            : 0;

        // Target Flags
        this.CheckTargetEnabled.IsChecked = (this._currentData.TargetFlags & 0x01) != 0;
        this.CheckTargetEnemies.IsChecked = (this._currentData.TargetFlags & 0x02) != 0;
        this.CheckTargetMulti.IsChecked = (this._currentData.TargetFlags & 0x04) != 0;
        this.CheckTargetSelfOnly.IsChecked = (this._currentData.TargetFlags & 0x08) != 0;
        this.CheckTargetEitherTeam.IsChecked = (this._currentData.TargetFlags & 0x20) != 0;
        this.CheckTargetDead.IsChecked = (this._currentData.TargetFlags & 0x40) != 0;
        this.CheckTargetLongRange.IsChecked = (this._currentData.TargetFlags & 0x80) != 0;

        // Damage Flags
        this.CheckDmgPhysical.IsChecked = (this._currentData.DamageFlags & 0x01) != 0;
        this.CheckDmgMagical.IsChecked = (this._currentData.DamageFlags & 0x02) != 0;
        this.CheckDmgCanCrit.IsChecked = (this._currentData.DamageFlags & 0x04) != 0;
        this.CheckDmgHeals.IsChecked = (this._currentData.DamageFlags & 0x10) != 0;
        this.CheckDmgCleanses.IsChecked = (this._currentData.DamageFlags & 0x20) != 0;
        this.CheckDmgBreaksLimit.IsChecked = (this._currentData.DamageFlags & 0x80) != 0;

        // Element Flags
        this.CheckElemFire.IsChecked = (this._currentData.ElementFlags & 0x01) != 0;
        this.CheckElemBlizzard.IsChecked = (this._currentData.ElementFlags & 0x02) != 0;
        this.CheckElemThunder.IsChecked = (this._currentData.ElementFlags & 0x04) != 0;
        this.CheckElemWater.IsChecked = (this._currentData.ElementFlags & 0x08) != 0;
        this.CheckElemHoly.IsChecked = (this._currentData.ElementFlags & 0x10) != 0;

        // Status Chances
        var sc = this._currentData.StatusChance ?? new byte[25];
        this.TextStatusDeath.Text = sc.Length > 0 ? sc[0].ToString() : "0";
        this.TextStatusZombie.Text = sc.Length > 1 ? sc[1].ToString() : "0";
        this.TextStatusPetrify.Text = sc.Length > 2 ? sc[2].ToString() : "0";
        this.TextStatusPoison.Text = sc.Length > 3 ? sc[3].ToString() : "0";
        this.TextStatusPowerBreak.Text = sc.Length > 4 ? sc[4].ToString() : "0";
        this.TextStatusMagicBreak.Text = sc.Length > 5 ? sc[5].ToString() : "0";
        this.TextStatusArmorBreak.Text = sc.Length > 6 ? sc[6].ToString() : "0";
        this.TextStatusMentalBreak.Text = sc.Length > 7 ? sc[7].ToString() : "0";
        this.TextStatusSleep.Text = sc.Length > 12 ? sc[12].ToString() : "0";
        this.TextStatusSilence.Text = sc.Length > 13 ? sc[13].ToString() : "0";
        this.TextStatusDarkness.Text = sc.Length > 14 ? sc[14].ToString() : "0";
        this.TextStatusSlow.Text = sc.Length > 24 ? sc[24].ToString() : "0";
    }

    void ButtonSave_Click(object sender, RoutedEventArgs e)
    {
        if (this._currentIndex < 0)
            return;

        try
        {
            this._currentData.CostMp = byte.Parse(this.TextCostMp.Text);
            this._currentData.CostOverdrive = byte.Parse(this.TextCostOverdrive.Text);
            this._currentData.MoveRank = byte.Parse(this.TextMoveRank.Text);
            this._currentData.AttackPower = byte.Parse(this.TextAttackPower.Text);
            this._currentData.AttackAccuracy = byte.Parse(this.TextAttackAccuracy.Text);
            this._currentData.HitCount = Math.Max((byte)1, byte.Parse(this.TextHitCount.Text));
            this._currentData.AttackCritBonus = byte.Parse(this.TextCritBonus.Text);
            this._currentData.ShatterChance = byte.Parse(this.TextShatterChance.Text);

            if (this.ComboDamageFormula.SelectedIndex >= 0)
            {
                this._currentData.DamageFormula = (byte)DamageFormula.DamageFormulas[this.ComboDamageFormula.SelectedIndex].ID;
            }

            this._currentData.CharacterUser = (sbyte)(this.ComboCharacterUser.SelectedIndex - 1);

            // Target Flags
            byte tf = 0;
            if (this.CheckTargetEnabled.IsChecked == true)
                tf |= 0x01;
            if (this.CheckTargetEnemies.IsChecked == true)
                tf |= 0x02;
            if (this.CheckTargetMulti.IsChecked == true)
                tf |= 0x04;
            if (this.CheckTargetSelfOnly.IsChecked == true)
                tf |= 0x08;
            if (this.CheckTargetEitherTeam.IsChecked == true)
                tf |= 0x20;
            if (this.CheckTargetDead.IsChecked == true)
                tf |= 0x40;
            if (this.CheckTargetLongRange.IsChecked == true)
                tf |= 0x80;
            this._currentData.TargetFlags = tf;

            // Damage Flags
            byte df = 0;
            if (this.CheckDmgPhysical.IsChecked == true)
                df |= 0x01;
            if (this.CheckDmgMagical.IsChecked == true)
                df |= 0x02;
            if (this.CheckDmgCanCrit.IsChecked == true)
                df |= 0x04;
            if (this.CheckDmgHeals.IsChecked == true)
                df |= 0x10;
            if (this.CheckDmgCleanses.IsChecked == true)
                df |= 0x20;
            if (this.CheckDmgBreaksLimit.IsChecked == true)
                df |= 0x80;
            this._currentData.DamageFlags = df;

            // Element Flags
            byte ef = 0;
            if (this.CheckElemFire.IsChecked == true)
                ef |= 0x01;
            if (this.CheckElemBlizzard.IsChecked == true)
                ef |= 0x02;
            if (this.CheckElemThunder.IsChecked == true)
                ef |= 0x04;
            if (this.CheckElemWater.IsChecked == true)
                ef |= 0x08;
            if (this.CheckElemHoly.IsChecked == true)
                ef |= 0x10;
            this._currentData.ElementFlags = ef;

            // Status Chances
            this._currentData.StatusChance ??= new byte[25];
            this._currentData.StatusChance[0] = byte.Parse(this.TextStatusDeath.Text);
            this._currentData.StatusChance[1] = byte.Parse(this.TextStatusZombie.Text);
            this._currentData.StatusChance[2] = byte.Parse(this.TextStatusPetrify.Text);
            this._currentData.StatusChance[3] = byte.Parse(this.TextStatusPoison.Text);
            this._currentData.StatusChance[4] = byte.Parse(this.TextStatusPowerBreak.Text);
            this._currentData.StatusChance[5] = byte.Parse(this.TextStatusMagicBreak.Text);
            this._currentData.StatusChance[6] = byte.Parse(this.TextStatusArmorBreak.Text);
            this._currentData.StatusChance[7] = byte.Parse(this.TextStatusMentalBreak.Text);
            this._currentData.StatusChance[12] = byte.Parse(this.TextStatusSleep.Text);
            this._currentData.StatusChance[13] = byte.Parse(this.TextStatusSilence.Text);
            this._currentData.StatusChance[14] = byte.Parse(this.TextStatusDarkness.Text);
            this._currentData.StatusChance[24] = byte.Parse(this.TextStatusSlow.Text);

            Skills.WriteData(this._currentIndex, this._currentData, this._currentType);

            MessageBox.Show("Changes written to game memory successfully!", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            Error.Show($"Error saving data:\n{ex.Message}");
        }
    }

    void ButtonRevert_Click(object sender, RoutedEventArgs e)
    {
        if (this._currentIndex >= 0)
        {
            this.LoadCommand(this._currentIndex, this._currentType);
        }
    }
}
