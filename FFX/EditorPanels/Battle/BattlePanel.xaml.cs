using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Farplane.Common;
using Farplane.FFX.Data;
using Farplane.FFX.Values;
using MahApps.Metro.Controls;

namespace Farplane.FFX.EditorPanels.Battle;

/// <summary>
/// Interaction logic for BattlePanel.xaml
/// </summary>
public partial class BattlePanel : UserControl
{
    bool _canWriteData = false;

    readonly BattleEntityData[] _partyEntities = new BattleEntityData[18];
    readonly BattleEntityData[] _enemyEntities = new BattleEntityData[8];

    int _enemyCount = 0;
    int _partyCount = 0;

    bool IsInBattle => BattleEntity.CheckBattleState();

    public BattlePanel()
    {
        this.InitializeComponent();

        foreach (var tabItem in this.TabBattle.Items)
        {
            ControlsHelper.SetHeaderFontSize((UIElement)tabItem, 14);
        }

        foreach (var tabItem in this.TabEntity.Items)
        {
            ControlsHelper.SetHeaderFontSize((UIElement)tabItem, 14);
        }

        this._canWriteData = true;
    }

    public void Refresh()
    {
        if (!this._canWriteData)
        {
            return;
        }

        if (!this.CheckBattleState())
        {
            return;
        }

        this._canWriteData = false;
        if (this.TabBattle.SelectedIndex == 0)
        {
            this.RefreshParty();
        }
        else
        {
            this.RefreshEnemies();
        }

        this.RefreshEntity();

        this._canWriteData = true;
    }

    bool CheckBattleState()
    {
        if (this.IsInBattle)
        {
            this.StackBattle.Visibility = Visibility.Visible;
            this.TextEnterBattle.Visibility = Visibility.Collapsed;
        }
        else
        {
            this.StackBattle.Visibility = Visibility.Collapsed;
            this.TextEnterBattle.Visibility = Visibility.Visible;
        }
        return this.IsInBattle;
    }

    public void RefreshEnemies()
    {
        this._enemyCount = 0;
        for (var i = 0; i < 8; i++)
        {
            var success = BattleEntity.ReadEntity(EntityType.Enemy, i, out var readEntity);

            this._enemyEntities[i] = readEntity;

            if (readEntity.pointer_1 == 0 || !success)
            {
                (this.TabEntity.Items[i] as TabItem).Visibility = Visibility.Collapsed;
                continue;
            }
            var entityTab = this.TabEntity.Items[i] as TabItem;
            entityTab.Visibility = Visibility.Visible;
            entityTab.Header = StringConverter.ToASCII(readEntity.text_name);

            this._enemyCount++;
        }
    }

    public void RefreshParty()
    {
        this._partyCount = 0;
        for (var i = 0; i < Enum.GetValues(typeof(Character)).Length - 1; i++)
        {
            var success = BattleEntity.ReadEntity(EntityType.Party, i, out var readEntity);
            this._partyEntities[i] = readEntity;

            if (readEntity.pointer_1 == 0 || !success)
            {
                (this.TabEntity.Items[i] as TabItem).Visibility = Visibility.Collapsed;
                continue;
            }

            var entityTab = this.TabEntity.Items[i] as TabItem;
            entityTab.Visibility = Visibility.Visible;
            entityTab.Header = ((Character)i).ToString();
            this._partyCount++;
        }
    }

    public void RefreshEntity()
    {
        //if (TabBattle.SelectedIndex == 0 && TabEntity.SelectedIndex >= _partyCount)
        //    TabEntity.SelectedIndex = 0;

        //if (TabBattle.SelectedIndex == 1 && TabEntity.SelectedIndex >= _enemyCount)
        //    TabEntity.SelectedIndex = 0;

        BattleEntity.ReadEntity(
            (EntityType)this.TabBattle.SelectedIndex,
            this.TabEntity.SelectedIndex,
            out var entityData
        );

        // Refresh stats panel
        this.TextCurrentHP.Text = entityData.hp_current.ToString();
        this.TextCurrentMP.Text = entityData.mp_current.ToString();
        this.TextMaxHP.Text = entityData.hp_max.ToString();
        this.TextMaxMP.Text = entityData.mp_max.ToString();
        this.TextOverdrive.Text = entityData.overdrive_current.ToString();
        this.TextOverdriveMax.Text = entityData.overdrive_max.ToString();
        this.TextStrength.Text = entityData.strength.ToString();
        this.TextDefense.Text = entityData.defense.ToString();
        this.TextMagic.Text = entityData.magic.ToString();
        this.TextMagicDefense.Text = entityData.magic_defense.ToString();
        this.TextAgility.Text = entityData.agility.ToString();
        this.TextLuck.Text = entityData.luck.ToString();
        this.TextEvasion.Text = entityData.evasion.ToString();
        this.TextAccuracy.Text = entityData.accuracy.ToString();

        // Refresh negative status checkboxes
        var statusFlags = BitHelper.GetBitArray(entityData.status_flags_negative);
        for (var i = 0; i < 16; i++)
        {
            var boxName = "CheckFlag" + (i + 1).ToString().Trim();
            var box = (CheckBox)this.FindName(boxName);
            if (box == null)
            {
                continue;
            }

            box.IsChecked = statusFlags[i];
        }

        this.CheckSilence.IsChecked = entityData.status_turns_silence != 0;
        this.TextSilence.Text = entityData.status_turns_silence.ToString();

        this.CheckDarkness.IsChecked = entityData.status_turns_darkness != 0;
        this.TextDarkness.Text = entityData.status_turns_darkness.ToString();

        this.CheckSleep.IsChecked = entityData.status_turns_sleep != 0;
        this.TextSleep.Text = entityData.status_turns_sleep.ToString();

        this.TextDoom.Text = entityData.timer_doom.ToString();

        // Refresh positive status checkboxes
        this.CheckShell.IsChecked = entityData.status_shell != 0;
        this.CheckProtect.IsChecked = entityData.status_protect != 0;
        this.CheckReflect.IsChecked = entityData.status_reflect != 0;
        this.CheckNulTide.IsChecked = entityData.status_nultide != 0;
        this.CheckNulBlaze.IsChecked = entityData.status_nulblaze != 0;
        this.CheckNulShock.IsChecked = entityData.status_nulshock != 0;
        this.CheckNulFrost.IsChecked = entityData.status_nulfrost != 0;
        this.CheckRegen.IsChecked = entityData.status_regen != 0;
        this.CheckHaste.IsChecked = entityData.status_haste != 0;
        this.CheckSlow.IsChecked = entityData.status_slow != 0;
        this.CheckUnknown.IsChecked = entityData.status_unknown != 0;

        var posFlags = BitHelper.GetBitArray(entityData.status_flags_positive);
        for (var i = 0; i < 16; i++)
        {
            var boxName = "CheckPositiveFlag" + (i + 1).ToString().Trim();
            var box = (CheckBox)this.FindName(boxName);
            if (box == null)
            {
                continue;
            }

            box.IsChecked = posFlags[i];
        }
    }

    void SetPositiveStatus(
        EntityType entityType,
        int entityIndex,
        string statusOffset,
        bool statusState
    )
    {
        if (!this._canWriteData)
        {
            return;
        }

        BattleEntity.WriteBytes(
            entityType,
            entityIndex,
            statusOffset,
            statusState ? (byte)0xFF : (byte)0
        );
    }

    void TabBattle_OnSelectionChanged(object sender, SelectionChangedEventArgs e) => this.Refresh();

    void TabEntity_OnSelectionChanged(object sender, SelectionChangedEventArgs e) => this.Refresh();

    void TextBoxStat_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        if (!this.CheckBattleState())
        {
            return;
        }

        if (!this._canWriteData)
        {
            return;
        }

        var senderBox = sender as TextBox;
        try
        {
            switch (senderBox.Name)
            {
                case "TextCurrentHP":
                    BattleEntity.WriteBytes(
                        (EntityType)this.TabBattle.SelectedIndex,
                        this.TabEntity.SelectedIndex,
                        nameof(BattleEntityData.hp_current),
                        BitConverter.GetBytes(int.Parse(senderBox.Text))
                    );
                    break;
                case "TextCurrentMP":
                    BattleEntity.WriteBytes(
                        (EntityType)this.TabBattle.SelectedIndex,
                        this.TabEntity.SelectedIndex,
                        nameof(BattleEntityData.mp_current),
                        BitConverter.GetBytes(int.Parse(senderBox.Text))
                    );
                    break;
                case "TextMaxHP":
                    BattleEntity.WriteBytes(
                        (EntityType)this.TabBattle.SelectedIndex,
                        this.TabEntity.SelectedIndex,
                        nameof(BattleEntityData.hp_max),
                        BitConverter.GetBytes(int.Parse(senderBox.Text))
                    );
                    break;

                case "TextMaxMP":
                    BattleEntity.WriteBytes(
                        (EntityType)this.TabBattle.SelectedIndex,
                        this.TabEntity.SelectedIndex,
                        nameof(BattleEntityData.mp_max),
                        BitConverter.GetBytes(int.Parse(senderBox.Text))
                    );
                    break;
                case "TextOverdrive":
                    BattleEntity.WriteBytes(
                        (EntityType)this.TabBattle.SelectedIndex,
                        this.TabEntity.SelectedIndex,
                        nameof(BattleEntityData.overdrive_current),
                        byte.Parse(senderBox.Text)
                    );
                    break;
                case "TextOverdriveMax":
                    BattleEntity.WriteBytes(
                        (EntityType)this.TabBattle.SelectedIndex,
                        this.TabEntity.SelectedIndex,
                        nameof(BattleEntityData.overdrive_max),
                        byte.Parse(senderBox.Text)
                    );
                    break;
                case "TextStrength":
                    BattleEntity.WriteBytes(
                        (EntityType)this.TabBattle.SelectedIndex,
                        this.TabEntity.SelectedIndex,
                        nameof(BattleEntityData.strength),
                        byte.Parse(senderBox.Text)
                    );
                    break;
                case "TextDefense":
                    BattleEntity.WriteBytes(
                        (EntityType)this.TabBattle.SelectedIndex,
                        this.TabEntity.SelectedIndex,
                        nameof(BattleEntityData.defense),
                        byte.Parse(senderBox.Text)
                    );
                    break;
                case "TextMagic":
                    BattleEntity.WriteBytes(
                        (EntityType)this.TabBattle.SelectedIndex,
                        this.TabEntity.SelectedIndex,
                        nameof(BattleEntityData.magic),
                        byte.Parse(senderBox.Text)
                    );
                    break;
                case "TextMagicDefense":
                    BattleEntity.WriteBytes(
                        (EntityType)this.TabBattle.SelectedIndex,
                        this.TabEntity.SelectedIndex,
                        nameof(BattleEntityData.magic_defense),
                        byte.Parse(senderBox.Text)
                    );
                    break;
                case "TextAgility":
                    BattleEntity.WriteBytes(
                        (EntityType)this.TabBattle.SelectedIndex,
                        this.TabEntity.SelectedIndex,
                        nameof(BattleEntityData.agility),
                        byte.Parse(senderBox.Text)
                    );
                    break;
                case "TextLuck":
                    BattleEntity.WriteBytes(
                        (EntityType)this.TabBattle.SelectedIndex,
                        this.TabEntity.SelectedIndex,
                        nameof(BattleEntityData.luck),
                        byte.Parse(senderBox.Text)
                    );
                    break;
                case "TextEvasion":
                    BattleEntity.WriteBytes(
                        (EntityType)this.TabBattle.SelectedIndex,
                        this.TabEntity.SelectedIndex,
                        nameof(BattleEntityData.evasion),
                        byte.Parse(senderBox.Text)
                    );
                    break;
                case "TextAccuracy":
                    BattleEntity.WriteBytes(
                        (EntityType)this.TabBattle.SelectedIndex,
                        this.TabEntity.SelectedIndex,
                        nameof(BattleEntityData.accuracy),
                        byte.Parse(senderBox.Text)
                    );
                    break;
                case "TextDoom":
                    BattleEntity.WriteBytes(
                        (EntityType)this.TabBattle.SelectedIndex,
                        this.TabEntity.SelectedIndex,
                        nameof(BattleEntityData.timer_doom),
                        byte.Parse(this.TextDoom.Text)
                    );
                    break;
                case "TextSilence":
                    BattleEntity.WriteBytes(
                        (EntityType)this.TabBattle.SelectedIndex,
                        this.TabEntity.SelectedIndex,
                        nameof(BattleEntityData.status_turns_silence),
                        byte.Parse(this.TextSilence.Text)
                    );
                    break;
                case "TextDarkness":
                    BattleEntity.WriteBytes(
                        (EntityType)this.TabBattle.SelectedIndex,
                        this.TabEntity.SelectedIndex,
                        nameof(BattleEntityData.status_turns_darkness),
                        byte.Parse(this.TextDarkness.Text)
                    );
                    break;
                case "TextSleep":
                    BattleEntity.WriteBytes(
                        (EntityType)this.TabBattle.SelectedIndex,
                        this.TabEntity.SelectedIndex,
                        nameof(BattleEntityData.status_turns_sleep),
                        byte.Parse(this.TextSleep.Text)
                    );
                    break;
            }
        }
        catch
        {
            Error.Show("The value you entered was invalid.");
        }
    }

    void CheckSleep_OnChecked(object sender, RoutedEventArgs e)
    {
        if (!this.CheckBattleState())
        {
            return;
        }

        if (!this._canWriteData)
        {
            return;
        }

        byte statusTurns = 0;

        if (this.CheckSleep.IsChecked.Value)
        {
            try
            {
                statusTurns = byte.Parse(this.TextSleep.Text);
            }
            catch { }

            if (statusTurns == 0)
            {
                statusTurns = 3;
            }
        }

        BattleEntity.WriteBytes(
            (EntityType)this.TabBattle.SelectedIndex,
            this.TabEntity.SelectedIndex,
            "status_turns_sleep",
            statusTurns
        );

        this.Refresh();
    }

    void CheckDarkness_OnChecked(object sender, RoutedEventArgs e)
    {
        if (!this.CheckBattleState())
        {
            return;
        }

        if (!this._canWriteData)
        {
            return;
        }

        byte statusTurns = 0;
        if (this.CheckDarkness.IsChecked.Value)
        {
            try
            {
                statusTurns = byte.Parse(this.TextDarkness.Text);
            }
            catch { }
            if (statusTurns == 0)
            {
                statusTurns = 3;
            }
        }

        BattleEntity.WriteBytes(
            (EntityType)this.TabBattle.SelectedIndex,
            this.TabEntity.SelectedIndex,
            "status_turns_darkness",
            statusTurns
        );

        this.Refresh();
    }

    void CheckSilence_OnChecked(object sender, RoutedEventArgs e)
    {
        if (!this.CheckBattleState())
        {
            return;
        }

        if (!this._canWriteData)
        {
            return;
        }

        byte statusTurns = 0;

        if (this.CheckSilence.IsChecked.Value)
        {
            try
            {
                statusTurns = byte.Parse(this.TextSilence.Text);
            }
            catch { }
            if (statusTurns == 0)
            {
                statusTurns = 3;
            }
        }

        BattleEntity.WriteBytes(
            (EntityType)this.TabBattle.SelectedIndex,
            this.TabEntity.SelectedIndex,
            "status_turns_silence",
            statusTurns
        );

        this.Refresh();
    }

    void CheckFlagNegative_Changed(object sender, RoutedEventArgs e)
    {
        if (!this._canWriteData)
        {
            return;
        }

        var name = (sender as CheckBox).Name;
        var index = int.Parse(name.Substring(9)) - 1;

        var byteIndex = index / 8;
        var bitIndex = index % 8;

        BattleEntity.ReadEntity(
            (EntityType)this.TabBattle.SelectedIndex,
            this.TabEntity.SelectedIndex,
            out var readEntity
        );

        var negativeFlags = readEntity.status_flags_negative;
        negativeFlags[byteIndex] = BitHelper.ToggleBit(negativeFlags[byteIndex], bitIndex);

        BattleEntity.WriteBytes(
            (EntityType)this.TabBattle.SelectedIndex,
            this.TabEntity.SelectedIndex,
            "status_flags_negative",
            negativeFlags
        );
    }

    void CheckShell_OnChecked(object sender, RoutedEventArgs e) =>
        this.SetPositiveStatus(
            (EntityType)this.TabBattle.SelectedIndex,
            this.TabEntity.SelectedIndex,
            "status_shell",
            this.CheckShell.IsChecked.Value
        );

    void CheckProtect_OnChecked(object sender, RoutedEventArgs e) =>
        this.SetPositiveStatus(
            (EntityType)this.TabBattle.SelectedIndex,
            this.TabEntity.SelectedIndex,
            "status_protect",
            this.CheckProtect.IsChecked.Value
        );

    void CheckReflect_OnChecked(object sender, RoutedEventArgs e) =>
        this.SetPositiveStatus(
            (EntityType)this.TabBattle.SelectedIndex,
            this.TabEntity.SelectedIndex,
            "status_reflect",
            this.CheckReflect.IsChecked.Value
        );

    void CheckNulTide_OnChecked(object sender, RoutedEventArgs e) =>
        this.SetPositiveStatus(
            (EntityType)this.TabBattle.SelectedIndex,
            this.TabEntity.SelectedIndex,
            "status_nultide",
            this.CheckNulTide.IsChecked.Value
        );

    void CheckNulBlaze_OnChecked(object sender, RoutedEventArgs e) =>
        this.SetPositiveStatus(
            (EntityType)this.TabBattle.SelectedIndex,
            this.TabEntity.SelectedIndex,
            "status_nulblaze",
            this.CheckNulBlaze.IsChecked.Value
        );

    void CheckNulShock_OnChecked(object sender, RoutedEventArgs e) =>
        this.SetPositiveStatus(
            (EntityType)this.TabBattle.SelectedIndex,
            this.TabEntity.SelectedIndex,
            "status_nulshock",
            this.CheckNulShock.IsChecked.Value
        );

    void CheckNulFrost_OnChecked(object sender, RoutedEventArgs e) =>
        this.SetPositiveStatus(
            (EntityType)this.TabBattle.SelectedIndex,
            this.TabEntity.SelectedIndex,
            "status_nulfrost",
            this.CheckNulFrost.IsChecked.Value
        );

    void CheckRegen_OnChecked(object sender, RoutedEventArgs e) =>
        this.SetPositiveStatus(
            (EntityType)this.TabBattle.SelectedIndex,
            this.TabEntity.SelectedIndex,
            "status_regen",
            this.CheckRegen.IsChecked.Value
        );

    void CheckHaste_OnChecked(object sender, RoutedEventArgs e) =>
        this.SetPositiveStatus(
            (EntityType)this.TabBattle.SelectedIndex,
            this.TabEntity.SelectedIndex,
            "status_haste",
            this.CheckHaste.IsChecked.Value
        );

    void CheckSlow_OnChecked(object sender, RoutedEventArgs e) =>
        this.SetPositiveStatus(
            (EntityType)this.TabBattle.SelectedIndex,
            this.TabEntity.SelectedIndex,
            "status_slow",
            this.CheckSlow.IsChecked.Value
        );

    void CheckUnknown_OnChecked(object sender, RoutedEventArgs e) =>
        this.SetPositiveStatus(
            (EntityType)this.TabBattle.SelectedIndex,
            this.TabEntity.SelectedIndex,
            "status_unknown",
            this.CheckUnknown.IsChecked.Value
        );

    void CheckPositiveFlag_OnChecked(object sender, RoutedEventArgs e)
    {
        if (!this._canWriteData)
        {
            return;
        }

        var name = (sender as CheckBox).Name;
        var index = int.Parse(name.Substring(17)) - 1;

        var byteIndex = index / 8;
        var bitIndex = index % 8;

        BattleEntity.ReadEntity(
            (EntityType)this.TabBattle.SelectedIndex,
            this.TabEntity.SelectedIndex,
            out var readEntity
        );

        var positiveFlags = readEntity.status_flags_positive;
        positiveFlags[byteIndex] = BitHelper.ToggleBit(positiveFlags[byteIndex], bitIndex);

        BattleEntity.WriteBytes(
            (EntityType)this.TabBattle.SelectedIndex,
            this.TabEntity.SelectedIndex,
            "status_flags_positive",
            positiveFlags
        );
    }
}
