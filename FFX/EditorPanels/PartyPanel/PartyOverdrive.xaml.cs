using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Farplane.Common;
using Farplane.FFX.Data;
using Farplane.FFX.Values;
using Farplane.Memory;

namespace Farplane.FFX.EditorPanels.PartyPanel;

public partial class PartyOverdrive : UserControl
{
    int _characterIndex = -1;
    bool _refreshing;
    int _offsetPartyStats => OffsetScanner.GetOffset(GameOffset.FFX_PartyStatBase);
    int _offsetOverdriveUnlock => OffsetScanner.GetOffset(GameOffset.FFX_OverdriveUnlockBase);
    readonly int _sizePartyMember = Marshal.SizeOf<PartyMember>();

    static readonly string[] TidusOverdriveNames =
    [
        "Spiral Cut",
        "Slice & Dice",
        "Energy Rain",
        "Blitz Ace"
    ];

    static readonly string[] AuronOverdriveNames =
    [
        "Dragon Fang",
        "Shooting Star",
        "Banishing Blade",
        "Tornado"
    ];

    static readonly string[] KimahriRageNames =
    [
        "Jump",
        "Fire Breath",
        "Seed Cannon",
        "Self-Destruct",
        "Thrust Kick",
        "Stone Breath",
        "Aqua Breath",
        "Doom",
        "White Wind",
        "Bad Breath",
        "Mighty Guard",
        "Nova"
    ];

    static readonly string[] WakkaReelNames =
    [
        "Element Reels",
        "Attack Reels",
        "Status Reels",
        "Aurochs Reels"
    ];

    public PartyOverdrive()
    {
        this.InitializeComponent();

        var gridRows = OverdriveMode.OverdriveModes.Length / 3;
        if (OverdriveMode.OverdriveModes.Length % 3 != 0)
        {
            gridRows++;
        }

        for (var i = 0; i < gridRows; i++)
        {
            this.GridOverdrive.RowDefinitions.Add(
                new RowDefinition { Height = new GridLength(30) }
            );
        }

        for (var i = 0; i < OverdriveMode.OverdriveModes.Length; i++)
        {
            var overdriveCheckBox = new CheckBox
            {
                Content = OverdriveMode.OverdriveModes[i].Name,
                VerticalAlignment = VerticalAlignment.Center,
            };
            overdriveCheckBox.Checked += (sender, args) => this.ToggleOverdrive(sender);
            overdriveCheckBox.Unchecked += (sender, args) => this.ToggleOverdrive(sender);

            var overdriveTextBox = new TextBox
            {
                Text = "999",
                Width = 50,
                HorizontalAlignment = HorizontalAlignment.Right,
                MaxLength = 5,
            };
            overdriveTextBox.KeyDown += this.OverdriveTextBox_KeyDown;

            var overdrivePanel = new DockPanel
            {
                Children = { overdriveCheckBox, overdriveTextBox },
                Width = 140,
                Height = 26,
                Margin = new Thickness(2),
            };

            Grid.SetColumn(overdrivePanel, i / gridRows);
            Grid.SetRow(overdrivePanel, i % gridRows);
            this.GridOverdrive.Children.Add(overdrivePanel);
        }

        this.ComboCurrentOverdrive.ItemsSource = OverdriveMode.OverdriveModes;
    }

    public void Refresh(int characterIndex)
    {
        this._refreshing = true;
        this._characterIndex = characterIndex;

        try
        {
            var totalOverdrives = OverdriveMode.OverdriveModes.Length;
            var charOffset = this._offsetPartyStats + (this._characterIndex * this._sizePartyMember);
            var offsetLevels = (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.OverdriveMode)) + charOffset;
            var offsetFlags = (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.OverdriveModes)) + charOffset;
            var offsetCounters = (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.OverdriveWarrior)) + charOffset;

            var odLevels = GameMemory.Read<byte>(offsetLevels, 3, false);
            var odBytes = GameMemory.Read<byte>(offsetFlags, 3, false);
            var odCounters = GameMemory.Read<byte>(offsetCounters, totalOverdrives * 2, false);

            if (odLevels != null && odLevels.Length >= 3)
            {
                this.ComboCurrentOverdrive.SelectedIndex = odLevels[0];
                this.TextOverdriveCurrent.Text = odLevels[1].ToString();
                this.TextOverdriveMax.Text = odLevels[2].ToString();
            }

            if (odBytes != null)
            {
                var learnedOverdrives = BitHelper.GetBitArray(odBytes, totalOverdrives);
                for (var i = 0; i < totalOverdrives; i++)
                {
                    if (this.GridOverdrive.Children[i] is not DockPanel dockPanel)
                    {
                        continue;
                    }
                    if (dockPanel.Children[0] is CheckBox checkLearned)
                    {
                        checkLearned.IsChecked = learnedOverdrives[OverdriveMode.OverdriveModes[i].BitIndex];
                    }
                    if (dockPanel.Children[1] is TextBox textCount && odCounters != null)
                    {
                        textCount.Text = BitConverter
                            .ToUInt16(odCounters, OverdriveMode.OverdriveModes[i].BitIndex * 2)
                            .ToString();
                    }
                }
            }

            // Refresh Dynamic Learned Overdrives / Ronso Rages
            this.RefreshLearnedOverdrives();
        }
        finally
        {
            this._refreshing = false;
        }
    }

    void RefreshLearnedOverdrives()
    {
        this.PanelLearnedCheckboxes.Children.Clear();
        var unlockBase = this._offsetOverdriveUnlock;
        var character = (Character)this._characterIndex;

        switch (character)
        {
            case Character.Tidus:
            {
                this.PanelOverdriveCondition.Visibility = Visibility.Visible;
                this.TextConditionLabel.Text = "Tidus Overdrive Uses:";
                var tidusUses = GameMemory.Read<int>(OffsetScanner.GetOffset(GameOffset.FFX_TidusOverdrive), false);
                this.TextConditionValue.Text = tidusUses.ToString();
                this.TextNoOverdrives.Visibility = Visibility.Collapsed;

                var unlockByte = GameMemory.Read<byte>(unlockBase, false);
                for (var i = 0; i < 4; i++)
                {
                    var isChecked = BitHelper.GetBit(unlockByte, i);
                    var bitIndex = i;
                    var cb = new CheckBox
                    {
                        Content = TidusOverdriveNames[i],
                        IsChecked = isChecked,
                        Width = 120,
                        Margin = new Thickness(4)
                    };
                    cb.Checked += (s, e) => this.UpdateByteBit(unlockBase, bitIndex, true);
                    cb.Unchecked += (s, e) => this.UpdateByteBit(unlockBase, bitIndex, false);
                    this.PanelLearnedCheckboxes.Children.Add(cb);
                }
                break;
            }
            case Character.Auron:
            {
                this.PanelOverdriveCondition.Visibility = Visibility.Collapsed;
                this.TextNoOverdrives.Visibility = Visibility.Collapsed;

                var unlockByte = GameMemory.Read<byte>(unlockBase, false);
                for (var i = 0; i < 4; i++)
                {
                    var bitIndex = i + 4; // Auron occupies bits 4..7
                    var isChecked = BitHelper.GetBit(unlockByte, bitIndex);
                    var cb = new CheckBox
                    {
                        Content = AuronOverdriveNames[i],
                        IsChecked = isChecked,
                        Width = 120,
                        Margin = new Thickness(4)
                    };
                    cb.Checked += (s, e) => this.UpdateByteBit(unlockBase, bitIndex, true);
                    cb.Unchecked += (s, e) => this.UpdateByteBit(unlockBase, bitIndex, false);
                    this.PanelLearnedCheckboxes.Children.Add(cb);
                }
                break;
            }
            case Character.Kimahri:
            {
                this.PanelOverdriveCondition.Visibility = Visibility.Collapsed;
                this.TextNoOverdrives.Visibility = Visibility.Collapsed;

                // 12 Ronso Rages span 12 bits starting at unlockBase + 1
                var ragesUshort = GameMemory.Read<ushort>(unlockBase + 1, false);
                for (var i = 0; i < 12; i++)
                {
                    var isChecked = (ragesUshort & (1 << i)) != 0;
                    var bitIndex = i;
                    var cb = new CheckBox
                    {
                        Content = KimahriRageNames[i],
                        IsChecked = isChecked,
                        Width = 120,
                        Margin = new Thickness(4)
                    };
                    cb.Checked += (s, e) => this.UpdateKimahriRage(unlockBase + 1, bitIndex, true);
                    cb.Unchecked += (s, e) => this.UpdateKimahriRage(unlockBase + 1, bitIndex, false);
                    this.PanelLearnedCheckboxes.Children.Add(cb);
                }
                break;
            }
            case Character.Wakka:
            {
                this.PanelOverdriveCondition.Visibility = Visibility.Visible;
                this.TextConditionLabel.Text = "Wakka Battles Won:";
                var wakkaWins = GameMemory.Read<int>(OffsetScanner.GetOffset(GameOffset.FFX_WakkaBattleCount), false);
                this.TextConditionValue.Text = wakkaWins.ToString();
                this.TextNoOverdrives.Visibility = Visibility.Collapsed;

                // Wakka occupies bits 4..7 of unlockBase + 2
                var wakkaByte = GameMemory.Read<byte>(unlockBase + 2, false);
                for (var i = 0; i < 4; i++)
                {
                    var bitIndex = i + 4;
                    var isChecked = BitHelper.GetBit(wakkaByte, bitIndex);
                    var cb = new CheckBox
                    {
                        Content = WakkaReelNames[i],
                        IsChecked = isChecked,
                        Width = 120,
                        Margin = new Thickness(4)
                    };
                    cb.Checked += (s, e) => this.UpdateWakkaReel(unlockBase + 2, bitIndex, true);
                    cb.Unchecked += (s, e) => this.UpdateWakkaReel(unlockBase + 2, bitIndex, false);
                    this.PanelLearnedCheckboxes.Children.Add(cb);
                }
                break;
            }
            default:
            {
                this.PanelOverdriveCondition.Visibility = Visibility.Collapsed;
                this.TextNoOverdrives.Visibility = Visibility.Visible;
                break;
            }
        }
    }

    void UpdateByteBit(int address, int bitIndex, bool value)
    {
        if (this._refreshing || !GameMemory.IsAttached)
        {
            return;
        }
        var b = GameMemory.Read<byte>(address, false);
        b = BitHelper.SetBit(b, bitIndex, value);
        GameMemory.Write(address, b, false);
    }

    void UpdateKimahriRage(int address, int bitIndex, bool value)
    {
        if (this._refreshing || !GameMemory.IsAttached)
        {
            return;
        }
        var cur = GameMemory.Read<ushort>(address, false);
        ushort updated;
        if (value)
        {
            updated = (ushort)(cur | (1 << bitIndex));
        }
        else
        {
            updated = (ushort)(cur & ~(1 << bitIndex));
        }
        // Bit-safety: Preserve top 4 bits (0xF000) which belong to Wakka
        updated = (ushort)((cur & 0xF000) | (updated & 0x0FFF));
        GameMemory.Write(address, updated, false);
    }

    void UpdateWakkaReel(int address, int bitIndex, bool value)
    {
        if (this._refreshing || !GameMemory.IsAttached)
        {
            return;
        }
        var cur = GameMemory.Read<byte>(address, false);
        byte updated;
        if (value)
        {
            updated = (byte)(cur | (1 << bitIndex));
        }
        else
        {
            updated = (byte)(cur & ~(1 << bitIndex));
        }
        // Bit-safety: Preserve bottom 4 bits (0x0F) which belong to Kimahri
        updated = (byte)((cur & 0x0F) | (updated & 0xF0));
        GameMemory.Write(address, updated, false);
    }

    void ButtonUnlockAllOverdrives_Click(object sender, RoutedEventArgs e)
    {
        if (!GameMemory.IsAttached)
        {
            return;
        }

        var unlockBase = this._offsetOverdriveUnlock;

        // 1. Tidus (bits 0..3) & Auron (bits 4..7) in byte 0 -> 0xFF
        GameMemory.Write(unlockBase, (byte)0xFF, false);

        // 2. Kimahri (bits 0..11) in ushort at +1 (preserve bits 12..15)
        var curKim = GameMemory.Read<ushort>(unlockBase + 1, false);
        var newKim = (ushort)((curKim & 0xF000) | 0x0FFF);
        GameMemory.Write(unlockBase + 1, newKim, false);

        // 3. Wakka (bits 4..7) in byte at +2 (preserve bits 0..3)
        var curWakka = GameMemory.Read<byte>(unlockBase + 2, false);
        var newWakka = (byte)((curWakka & 0x0F) | 0xF0);
        GameMemory.Write(unlockBase + 2, newWakka, false);

        this.Refresh(this._characterIndex);
        MessageBox.Show(
            "All unique Overdrives, Reels, and Ronso Rages for Tidus, Auron, Kimahri, and Wakka have been unlocked!",
            "Overdrives Unlocked",
            MessageBoxButton.OK,
            MessageBoxImage.Information
        );
    }

    void TextConditionValue_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || this._refreshing || !GameMemory.IsAttached)
        {
            return;
        }

        try
        {
            var character = (Character)this._characterIndex;
            var val = int.Parse(this.TextConditionValue.Text);
            if (character == Character.Tidus)
            {
                GameMemory.Write(OffsetScanner.GetOffset(GameOffset.FFX_TidusOverdrive), val, false);
            }
            else if (character == Character.Wakka)
            {
                GameMemory.Write(OffsetScanner.GetOffset(GameOffset.FFX_WakkaBattleCount), val, false);
            }
            this.TextConditionValue.SelectAll();
        }
        catch
        {
            Error.Show("Please enter a valid numeric value.");
        }
    }

    void OverdriveTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || this._refreshing)
        {
            return;
        }
        try
        {
            if (sender is not TextBox textBox)
            {
                return;
            }
            var writeOffset = this._offsetPartyStats + (this._characterIndex * this._sizePartyMember);
            switch (textBox.Name)
            {
                case "TextOverdriveCurrent":
                    writeOffset += (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.OverdriveLevel));
                    GameMemory.Write(writeOffset, byte.Parse(this.TextOverdriveCurrent.Text), false);
                    this.TextOverdriveCurrent.SelectAll();
                    break;
                case "TextOverdriveMax":
                    writeOffset += (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.OverdriveMax));
                    GameMemory.Write(writeOffset, byte.Parse(this.TextOverdriveMax.Text), false);
                    this.TextOverdriveMax.SelectAll();
                    break;
                default:
                    this.SetOverdriveCount(sender);
                    break;
            }
        }
        catch
        {
            Error.Show("Please enter a value between 0 and 255.");
        }
    }

    public void ToggleOverdrive(object sender)
    {
        if (this._refreshing)
        {
            return;
        }
        if (sender is CheckBox checkBox)
        {
            if (checkBox.Parent is not DockPanel panel)
            {
                return;
            }
            var odIndex = this.GridOverdrive.Children.IndexOf(panel);
            OverdriveMode.ToggleOverdriveMode(
                this._characterIndex,
                OverdriveMode.OverdriveModes[odIndex].BitIndex
            );
        }
        this.Refresh(this._characterIndex);
    }

    public void SetOverdriveCount(object sender)
    {
        if (this._refreshing)
        {
            return;
        }
        var textBox = sender as TextBox;
        if (textBox != null)
        {
            if (textBox.Parent is not DockPanel panel)
            {
                return;
            }
            var odIndex = this.GridOverdrive.Children.IndexOf(panel);
            try
            {
                var odCount = ushort.Parse(textBox.Text);
                OverdriveMode.SetOverdriveCounter(
                    this._characterIndex,
                    OverdriveMode.OverdriveModes[odIndex].BitIndex,
                    odCount
                );
            }
            catch
            {
                Error.Show("Please enter a value between 0 and 65535.");
                return;
            }
        }
        this.Refresh(this._characterIndex);
        textBox?.SelectAll();
    }

    void ButtonMax_Click(object sender, RoutedEventArgs e)
    {
        var charOffset = this._offsetPartyStats + (this._characterIndex * this._sizePartyMember);
        var levelOffset = (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.OverdriveLevel)) + charOffset;
        var currentMax = GameMemory.Read<byte>(levelOffset, 2, false);
        if (currentMax != null && currentMax.Length >= 2)
        {
            GameMemory.Write(levelOffset, currentMax[1], false);
        }
        this.Refresh(this._characterIndex);
    }

    void ComboCurrentOverdrive_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (this._refreshing)
        {
            return;
        }
        var charOffset = this._offsetPartyStats + (this._characterIndex * this._sizePartyMember);
        var offset = (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.OverdriveMode)) + charOffset;
        if (this.ComboCurrentOverdrive.SelectedIndex >= 0)
        {
            GameMemory.Write(
                offset,
                (byte)OverdriveMode.OverdriveModes[this.ComboCurrentOverdrive.SelectedIndex].BitIndex,
                false
            );
        }
    }
}
