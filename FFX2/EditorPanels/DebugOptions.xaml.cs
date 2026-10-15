using System;
using System.Windows;
using System.Windows.Controls;

namespace Farplane.FFX2.EditorPanels;

/// <summary>
/// Interaction logic for DebugOptions.xaml
/// </summary>
public partial class DebugOptions : UserControl
{
    bool refreshing = false;

    public DebugOptions() => this.InitializeComponent();

    public void Refresh()
    {
        this.refreshing = true;

        this.CheckAllyInvincible.IsChecked = LegacyMemoryReader.ReadByteFlag(
            (int)Offsets.DebugFlags.AllyInvincible
        );
        this.CheckEnemyInvincible.IsChecked = LegacyMemoryReader.ReadByteFlag(
            (int)Offsets.DebugFlags.EnemyInvincible
        );
        this.CheckControlEnemies.IsChecked = LegacyMemoryReader.ReadByteFlag(
            (int)Offsets.DebugFlags.ControlEnemies
        );
        this.CheckControlMonsters.IsChecked = LegacyMemoryReader.ReadByteFlag(
            (int)Offsets.DebugFlags.ControlMonsters
        );
        this.CheckZeroMP.IsChecked = LegacyMemoryReader.ReadByteFlag(
            (int)Offsets.DebugFlags.MPZero
        );
        this.CheckInfoOutput.IsChecked = LegacyMemoryReader.ReadByteFlag(
            (int)Offsets.DebugFlags.InfoOutput
        );
        this.CheckAlwaysCritical.IsChecked = LegacyMemoryReader.ReadByteFlag(
            (int)Offsets.DebugFlags.AlwaysCritical
        );
        this.CheckCritical.IsChecked = LegacyMemoryReader.ReadByteFlag(
            (int)Offsets.DebugFlags.Critical
        );
        this.CheckProbability.IsChecked = LegacyMemoryReader.ReadByteFlag(
            (int)Offsets.DebugFlags.Probability100
        );
        this.CheckDamageRandom.IsChecked = LegacyMemoryReader.ReadByteFlag(
            (int)Offsets.DebugFlags.DamageRandom
        );
        this.CheckDamage1.IsChecked = LegacyMemoryReader.ReadByteFlag(
            (int)Offsets.DebugFlags.Damage1
        );
        this.CheckDamage9999.IsChecked = LegacyMemoryReader.ReadByteFlag(
            (int)Offsets.DebugFlags.Damage9999
        );
        this.CheckDamage99999.IsChecked = LegacyMemoryReader.ReadByteFlag(
            (int)Offsets.DebugFlags.Damage99999
        );
        this.CheckRareDrop.IsChecked = LegacyMemoryReader.ReadByteFlag(
            (int)Offsets.DebugFlags.RareDrop100
        );
        this.CheckEXP100x.IsChecked = LegacyMemoryReader.ReadByteFlag(
            (int)Offsets.DebugFlags.EXP100x
        );
        this.CheckGil100x.IsChecked = LegacyMemoryReader.ReadByteFlag(
            (int)Offsets.DebugFlags.Gil100x
        );
        this.CheckAlwaysOversoul.IsChecked = LegacyMemoryReader.ReadByteFlag(
            (int)Offsets.DebugFlags.AlwaysOversoul
        );

        var firstAttack = LegacyMemoryReader.ReadByte((int)Offsets.DebugFlags.FirstAttack);
        this.CheckAttackFirst.IsChecked = firstAttack != 0xFF;

        this.refreshing = false;
    }

    void CheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (this.refreshing)
        {
            return;
        }

        var checkBox = (CheckBox)sender;
        var checkedBytes = checkBox.IsChecked == true ? new byte[] { 1 } : [0];

        if ((string)checkBox.Tag == "FirstAttack")
        {
            checkedBytes = checkBox.IsChecked == true ? [0x00] : [0xFF];
        }

        try
        {
            var offset = Enum.Parse(typeof(Offsets.DebugFlags), (string)checkBox.Tag);
            LegacyMemoryReader.WriteBytes((int)offset, checkedBytes);
        }
        catch { }
    }
}
