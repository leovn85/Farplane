using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Farplane.Common;
using Farplane.FFX.Data;
using Farplane.FFX.Values;
using Farplane.Memory;
using MahApps.Metro.Controls;

namespace Farplane.FFX.EditorPanels.Battle;

public partial class BattlePanel : UserControl
{
    bool _canWriteData;
    readonly BattleEntityData[] _partyEntities = new BattleEntityData[18];
    readonly BattleEntityData[] _enemyEntities = new BattleEntityData[8];
    readonly DispatcherTimer _autoRefreshTimer;

    bool IsInBattle => BattleEntity.CheckBattleState();

    public BattlePanel()
    {
        this.InitializeComponent();

        foreach (var tabItem in this.TabBattle.Items)
        {
            ControlsHelper.SetHeaderFontSize((UIElement)tabItem, 13);
        }
        foreach (var tabItem in this.TabEntity.Items)
        {
            ControlsHelper.SetHeaderFontSize((UIElement)tabItem, 12);
        }

        this._autoRefreshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        this._autoRefreshTimer.Tick += this.AutoRefreshTimer_Tick;

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
        try
        {
            if (this.TabBattle.SelectedIndex == 0)
            {
                this.RefreshParty();
            }
            else
            {
                this.RefreshEnemies();
            }

            this.RefreshEntity();
        }
        finally
        {
            this._canWriteData = true;
        }
    }

    bool CheckBattleState()
    {
        var inBattle = this.IsInBattle;
        if (inBattle)
        {
            this.StackBattle.Visibility = Visibility.Visible;
            this.TextEnterBattle.Visibility = Visibility.Collapsed;
        }
        else
        {
            this.StackBattle.Visibility = Visibility.Collapsed;
            this.TextEnterBattle.Visibility = Visibility.Visible;
        }
        return inBattle;
    }

    public void RefreshEnemies()
    {
        for (var i = 0; i < 8; i++)
        {
            var success = BattleEntity.ReadEntity(EntityType.Enemy, i, out var readEntity);
            this._enemyEntities[i] = readEntity;

            var tabItem = this.TabEntity.Items[i] as TabItem;
            if (tabItem == null)
                continue;

            if (readEntity.pointer_1 == 0 || !success)
            {
                tabItem.Visibility = Visibility.Collapsed;
                continue;
            }

            tabItem.Visibility = Visibility.Visible;
            var enemyName = StringConverter.ToASCII(readEntity.text_name);
            tabItem.Header = string.IsNullOrWhiteSpace(enemyName) ? $"Enemy {i + 1}" : enemyName;
        }
    }

    public void RefreshParty()
    {
        var totalParty = Enum.GetValues(typeof(Character)).Length - 1; // Exclude None
        for (var i = 0; i < 8; i++)
        {
            var tabItem = this.TabEntity.Items[i] as TabItem;
            if (tabItem == null)
                continue;

            if (i >= totalParty)
            {
                tabItem.Visibility = Visibility.Collapsed;
                continue;
            }

            var success = BattleEntity.ReadEntity(EntityType.Party, i, out var readEntity);
            this._partyEntities[i] = readEntity;

            if (readEntity.pointer_1 == 0 || !success)
            {
                tabItem.Visibility = Visibility.Collapsed;
                continue;
            }

            tabItem.Visibility = Visibility.Visible;
            tabItem.Header = ((Character)i).ToString();
        }
    }

    public void RefreshEntity()
    {
        var entityType = (EntityType)this.TabBattle.SelectedIndex;
        var entityIndex = Math.Max(0, this.TabEntity.SelectedIndex);

        BattleEntity.ReadEntity(entityType, entityIndex, out var entityData);

        // Vitals & Turn Gauges
        this.TextCurrentHP.Text = entityData.hp_current.ToString();
        this.TextMaxHP.Text = entityData.hp_max.ToString();
        this.TextCurrentMP.Text = entityData.mp_current.ToString();
        this.TextMaxMP.Text = entityData.mp_max.ToString();
        this.TextOverkill.Text = entityData.overkill.ToString();

        this.TextCTB.Text = entityData.ctb.ToString();
        this.TextMaxCTB.Text = entityData.max_ctb.ToString();

        this.TextOverdrive.Text = entityData.overdrive_current.ToString();
        this.TextOverdriveMax.Text = entityData.overdrive_max.ToString();

        this.TextStealCount.Text = entityData.steal_count.ToString();
        this.TextBribeGil.Text = entityData.bribe_gil_spent.ToString();

        // Base Attributes
        this.TextStrength.Text = entityData.strength.ToString();
        this.TextDefense.Text = entityData.defense.ToString();
        this.TextMagic.Text = entityData.magic.ToString();
        this.TextMagicDefense.Text = entityData.magic_defense.ToString();
        this.TextAgility.Text = entityData.agility.ToString();
        this.TextLuck.Text = entityData.luck.ToString();
        this.TextEvasion.Text = entityData.evasion.ToString();
        this.TextAccuracy.Text = entityData.accuracy.ToString();

        // Stat Boosts (In-Battle Ups)
        this.TextStrengthUp.Text = entityData.strength_up.ToString();
        this.TextDefenseUp.Text = entityData.defense_up.ToString();
        this.TextMagicUp.Text = entityData.magic_up.ToString();
        this.TextMagicDefUp.Text = entityData.magic_defense_up.ToString();
        this.TextAgilityUp.Text = entityData.agility_up.ToString();
        this.TextLuckUp.Text = entityData.luck_up.ToString();
        this.TextEvasionUp.Text = entityData.evasion_up.ToString();
        this.TextAccuracyUp.Text = entityData.accuracy_up.ToString();

        // Stacks (0 - 5)
        this.TextCheerStacks.Text = entityData.cheer_stacks.ToString();
        this.TextAimStacks.Text = entityData.aim_stacks.ToString();
        this.TextFocusStacks.Text = entityData.focus_stacks.ToString();
        this.TextReflexStacks.Text = entityData.reflex_stacks.ToString();
        this.TextLuckStacks.Text = entityData.luck_stacks.ToString();
        this.TextJinxStacks.Text = entityData.jinx_stacks.ToString();

        // Negative Ailments
        var negBytes = BitConverter.GetBytes(entityData.status_flags_negative);
        var negFlags = BitHelper.GetBitArray(negBytes, 16);
        for (var i = 0; i < 12; i++)
        {
            var box = (CheckBox)this.FindName($"CheckFlag{i + 1}");
            if (box != null)
            {
                box.IsChecked = negFlags[i];
            }
        }

        this.CheckSilence.IsChecked = entityData.status_turns_silence != 0;
        this.TextSilence.Text = entityData.status_turns_silence.ToString();
        this.CheckDarkness.IsChecked = entityData.status_turns_darkness != 0;
        this.TextDarkness.Text = entityData.status_turns_darkness.ToString();
        this.CheckSleep.IsChecked = entityData.status_turns_sleep != 0;
        this.TextSleep.Text = entityData.status_turns_sleep.ToString();
        this.TextDoom.Text = entityData.timer_doom.ToString();

        // Positive & Defensive Statuses
        this.CheckShell.IsChecked = entityData.status_shell != 0;
        this.CheckProtect.IsChecked = entityData.status_protect != 0;
        this.CheckReflect.IsChecked = entityData.status_reflect != 0;
        this.CheckRegen.IsChecked = entityData.status_regen != 0;
        this.CheckHaste.IsChecked = entityData.status_haste != 0;
        this.CheckSlow.IsChecked = entityData.status_slow != 0;

        this.CheckNulTide.IsChecked = entityData.status_nultide != 0;
        this.CheckNulBlaze.IsChecked = entityData.status_nulblaze != 0;
        this.CheckNulShock.IsChecked = entityData.status_nulshock != 0;
        this.CheckNulFrost.IsChecked = entityData.status_nulfrost != 0;

        var posBytes = BitConverter.GetBytes(entityData.status_flags_positive);
        var posFlags = BitHelper.GetBitArray(posBytes, 16);
        for (var i = 0; i < 16; i++)
        {
            var box = (CheckBox)this.FindName($"CheckPositiveFlag{i + 1}");
            if (box != null)
            {
                box.IsChecked = posFlags[i];
            }
        }
    }

    void SetPositiveStatus(string statusOffset, bool statusState)
    {
        if (!this._canWriteData)
            return;
        BattleEntity.WriteBytes(
            (EntityType)this.TabBattle.SelectedIndex,
            this.TabEntity.SelectedIndex,
            statusOffset,
            statusState ? (byte)0xFF : (byte)0
        );
    }

    void TabBattle_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.Source is TabControl)
            this.Refresh();
    }

    void TabEntity_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.Source is TabControl)
            this.Refresh();
    }

    void ButtonRefresh_Click(object sender, RoutedEventArgs e) => this.Refresh();

    void CheckAutoRefresh_Changed(object sender, RoutedEventArgs e)
    {
        if (this.CheckAutoRefresh.IsChecked == true)
        {
            this._autoRefreshTimer.Start();
        }
        else
        {
            this._autoRefreshTimer.Stop();
        }
    }

    void AutoRefreshTimer_Tick(object sender, EventArgs e)
    {
        if (!GameMemory.IsAttached || !this.IsInBattle)
        {
            return;
        }

        // Skip refresh tick while user is actively typing in any TextBox
        var focused = FocusManager.GetFocusedElement(Window.GetWindow(this));
        if (focused is TextBox)
        {
            return;
        }

        this.Refresh();
    }

    void TextBoxStat_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || !this.CheckBattleState() || !this._canWriteData)
        {
            return;
        }

        if (sender is not TextBox senderBox)
            return;

        var entityType = (EntityType)this.TabBattle.SelectedIndex;
        var entityIndex = this.TabEntity.SelectedIndex;

        try
        {
            switch (senderBox.Name)
            {
                // Vitals
                case "TextCurrentHP":
                    BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.hp_current), BitConverter.GetBytes(int.Parse(senderBox.Text)));
                    break;
                case "TextMaxHP":
                    BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.hp_max), BitConverter.GetBytes(int.Parse(senderBox.Text)));
                    BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.hp_max2), BitConverter.GetBytes(int.Parse(senderBox.Text)));
                    break;
                case "TextCurrentMP":
                    BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.mp_current), BitConverter.GetBytes(int.Parse(senderBox.Text)));
                    break;
                case "TextMaxMP":
                    BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.mp_max), BitConverter.GetBytes(int.Parse(senderBox.Text)));
                    BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.mp_max2), BitConverter.GetBytes(int.Parse(senderBox.Text)));
                    break;
                case "TextOverkill":
                    BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.overkill), BitConverter.GetBytes(int.Parse(senderBox.Text)));
                    break;
                case "TextCTB":
                    BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.ctb), byte.Parse(senderBox.Text));
                    break;
                case "TextMaxCTB":
                    BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.max_ctb), byte.Parse(senderBox.Text));
                    break;
                case "TextOverdrive":
                    BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.overdrive_current), byte.Parse(senderBox.Text));
                    break;
                case "TextOverdriveMax":
                    BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.overdrive_max), byte.Parse(senderBox.Text));
                    break;
                case "TextStealCount":
                    BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.steal_count), byte.Parse(senderBox.Text));
                    break;
                case "TextBribeGil":
                    BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.bribe_gil_spent), BitConverter.GetBytes(uint.Parse(senderBox.Text)));
                    break;

                // Base Attributes
                case "TextStrength":
                    BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.strength), byte.Parse(senderBox.Text));
                    break;
                case "TextDefense":
                    BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.defense), byte.Parse(senderBox.Text));
                    break;
                case "TextMagic":
                    BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.magic), byte.Parse(senderBox.Text));
                    break;
                case "TextMagicDefense":
                    BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.magic_defense), byte.Parse(senderBox.Text));
                    break;
                case "TextAgility":
                    BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.agility), byte.Parse(senderBox.Text));
                    break;
                case "TextLuck":
                    BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.luck), byte.Parse(senderBox.Text));
                    break;
                case "TextEvasion":
                    BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.evasion), byte.Parse(senderBox.Text));
                    break;
                case "TextAccuracy":
                    BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.accuracy), byte.Parse(senderBox.Text));
                    break;

                // Stat Boosts (Ups)
                case "TextStrengthUp":
                    BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.strength_up), byte.Parse(senderBox.Text));
                    break;
                case "TextDefenseUp":
                    BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.defense_up), byte.Parse(senderBox.Text));
                    break;
                case "TextMagicUp":
                    BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.magic_up), byte.Parse(senderBox.Text));
                    break;
                case "TextMagicDefUp":
                    BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.magic_defense_up), byte.Parse(senderBox.Text));
                    break;
                case "TextAgilityUp":
                    BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.agility_up), byte.Parse(senderBox.Text));
                    break;
                case "TextLuckUp":
                    BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.luck_up), byte.Parse(senderBox.Text));
                    break;
                case "TextEvasionUp":
                    BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.evasion_up), byte.Parse(senderBox.Text));
                    break;
                case "TextAccuracyUp":
                    BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.accuracy_up), byte.Parse(senderBox.Text));
                    break;

                // Stacks (0-5)
                case "TextCheerStacks":
                    BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.cheer_stacks), byte.Parse(senderBox.Text));
                    break;
                case "TextAimStacks":
                    BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.aim_stacks), byte.Parse(senderBox.Text));
                    break;
                case "TextFocusStacks":
                    BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.focus_stacks), byte.Parse(senderBox.Text));
                    break;
                case "TextReflexStacks":
                    BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.reflex_stacks), byte.Parse(senderBox.Text));
                    break;
                case "TextLuckStacks":
                    BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.luck_stacks), byte.Parse(senderBox.Text));
                    break;
                case "TextJinxStacks":
                    BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.jinx_stacks), byte.Parse(senderBox.Text));
                    break;

                // Turns Left
                case "TextDoom":
                    BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.timer_doom), byte.Parse(senderBox.Text));
                    break;
                case "TextSilence":
                    BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.status_turns_silence), byte.Parse(senderBox.Text));
                    break;
                case "TextDarkness":
                    BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.status_turns_darkness), byte.Parse(senderBox.Text));
                    break;
                case "TextSleep":
                    BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.status_turns_sleep), byte.Parse(senderBox.Text));
                    break;
            }

            senderBox.SelectAll();
        }
        catch
        {
            Error.Show("The value you entered was invalid.");
        }
    }

    void CheckSleep_OnChecked(object sender, RoutedEventArgs e)
    {
        if (!this.CheckBattleState() || !this._canWriteData)
            return;
        byte turns = 0;
        if (this.CheckSleep.IsChecked == true)
        {
            byte.TryParse(this.TextSleep.Text, out turns);
            if (turns == 0)
                turns = 3;
        }
        BattleEntity.WriteBytes((EntityType)this.TabBattle.SelectedIndex, this.TabEntity.SelectedIndex, nameof(BattleEntityData.status_turns_sleep), turns);
        this.Refresh();
    }

    void CheckDarkness_OnChecked(object sender, RoutedEventArgs e)
    {
        if (!this.CheckBattleState() || !this._canWriteData)
            return;
        byte turns = 0;
        if (this.CheckDarkness.IsChecked == true)
        {
            byte.TryParse(this.TextDarkness.Text, out turns);
            if (turns == 0)
                turns = 3;
        }
        BattleEntity.WriteBytes((EntityType)this.TabBattle.SelectedIndex, this.TabEntity.SelectedIndex, nameof(BattleEntityData.status_turns_darkness), turns);
        this.Refresh();
    }

    void CheckSilence_OnChecked(object sender, RoutedEventArgs e)
    {
        if (!this.CheckBattleState() || !this._canWriteData)
            return;
        byte turns = 0;
        if (this.CheckSilence.IsChecked == true)
        {
            byte.TryParse(this.TextSilence.Text, out turns);
            if (turns == 0)
                turns = 3;
        }
        BattleEntity.WriteBytes((EntityType)this.TabBattle.SelectedIndex, this.TabEntity.SelectedIndex, nameof(BattleEntityData.status_turns_silence), turns);
        this.Refresh();
    }

    void CheckFlagNegative_Changed(object sender, RoutedEventArgs e)
    {
        if (!this._canWriteData)
            return;
        if (sender is not CheckBox checkBox)
            return;

        var index = int.Parse(checkBox.Name.Substring(9)) - 1;
        var entityType = (EntityType)this.TabBattle.SelectedIndex;
        var entityIndex = this.TabEntity.SelectedIndex;

        BattleEntity.ReadEntity(entityType, entityIndex, out var readEntity);
        var flags = readEntity.status_flags_negative;

        if (checkBox.IsChecked == true)
            flags |= (ushort)(1 << index);
        else
            flags &= (ushort)~(1 << index);

        BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.status_flags_negative), BitConverter.GetBytes(flags));
    }

    void CheckShell_OnChecked(object sender, RoutedEventArgs e) => this.SetPositiveStatus(nameof(BattleEntityData.status_shell), this.CheckShell.IsChecked == true);
    void CheckProtect_OnChecked(object sender, RoutedEventArgs e) => this.SetPositiveStatus(nameof(BattleEntityData.status_protect), this.CheckProtect.IsChecked == true);
    void CheckReflect_OnChecked(object sender, RoutedEventArgs e) => this.SetPositiveStatus(nameof(BattleEntityData.status_reflect), this.CheckReflect.IsChecked == true);
    void CheckRegen_OnChecked(object sender, RoutedEventArgs e) => this.SetPositiveStatus(nameof(BattleEntityData.status_regen), this.CheckRegen.IsChecked == true);
    void CheckHaste_OnChecked(object sender, RoutedEventArgs e) => this.SetPositiveStatus(nameof(BattleEntityData.status_haste), this.CheckHaste.IsChecked == true);
    void CheckSlow_OnChecked(object sender, RoutedEventArgs e) => this.SetPositiveStatus(nameof(BattleEntityData.status_slow), this.CheckSlow.IsChecked == true);

    void CheckNulTide_OnChecked(object sender, RoutedEventArgs e) => this.SetPositiveStatus(nameof(BattleEntityData.status_nultide), this.CheckNulTide.IsChecked == true);
    void CheckNulBlaze_OnChecked(object sender, RoutedEventArgs e) => this.SetPositiveStatus(nameof(BattleEntityData.status_nulblaze), this.CheckNulBlaze.IsChecked == true);
    void CheckNulShock_OnChecked(object sender, RoutedEventArgs e) => this.SetPositiveStatus(nameof(BattleEntityData.status_nulshock), this.CheckNulShock.IsChecked == true);
    void CheckNulFrost_OnChecked(object sender, RoutedEventArgs e) => this.SetPositiveStatus(nameof(BattleEntityData.status_nulfrost), this.CheckNulFrost.IsChecked == true);

    void CheckPositiveFlag_OnChecked(object sender, RoutedEventArgs e)
    {
        if (!this._canWriteData)
            return;
        if (sender is not CheckBox checkBox)
            return;

        var index = int.Parse(checkBox.Name.Substring(17)) - 1;
        var entityType = (EntityType)this.TabBattle.SelectedIndex;
        var entityIndex = this.TabEntity.SelectedIndex;

        BattleEntity.ReadEntity(entityType, entityIndex, out var readEntity);
        var flags = readEntity.status_flags_positive;

        if (checkBox.IsChecked == true)
            flags |= (ushort)(1 << index);
        else
            flags &= (ushort)~(1 << index);

        BattleEntity.WriteBytes(entityType, entityIndex, nameof(BattleEntityData.status_flags_positive), BitConverter.GetBytes(flags));
    }
}
