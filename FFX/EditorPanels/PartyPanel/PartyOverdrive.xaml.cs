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

/// <summary>
/// Interaction logic for PartyOverdrive.xaml
/// </summary>
public partial class PartyOverdrive : UserControl
{
    int _characterIndex = -1;
    bool _refreshing = false;
    int _offsetPartyStats => OffsetScanner.GetOffset(GameOffset.FFX_PartyStatBase);
    readonly int _sizePartyMember = Marshal.SizeOf<PartyMember>();

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
            var overdriveCheckBox = new CheckBox()
            {
                Content = OverdriveMode.OverdriveModes[i].Name,
                VerticalAlignment = VerticalAlignment.Center,
            };
            overdriveCheckBox.Checked += (sender, args) => this.ToggleOverdrive(sender);
            overdriveCheckBox.Unchecked += (sender, args) => this.ToggleOverdrive(sender);
            var overdriveTextBox = new TextBox()
            {
                Text = "999",
                Width = 50,
                HorizontalAlignment = HorizontalAlignment.Right,
                MaxLength = 5,
            };
            overdriveTextBox.KeyDown += this.OverdriveTextBox_KeyDown;
            var overdrivePanel = new DockPanel()
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
            var writeOffset =
                this._offsetPartyStats + (this._characterIndex * this._sizePartyMember);
            switch (textBox.Name)
            {
                case "TextOverdriveCurrent":
                    writeOffset += (int)
                        Marshal.OffsetOf<PartyMember>(nameof(PartyMember.OverdriveLevel));
                    GameMemory.Write(
                        writeOffset,
                        byte.Parse(this.TextOverdriveCurrent.Text),
                        false
                    );
                    this.TextOverdriveCurrent.SelectAll();
                    break;
                case "TextOverdriveMax":
                    writeOffset += (int)
                        Marshal.OffsetOf<PartyMember>(nameof(PartyMember.OverdriveMax));
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
                int odCount = ushort.Parse(textBox.Text);
                OverdriveMode.SetOverdriveCounter(
                    this._characterIndex,
                    OverdriveMode.OverdriveModes[odIndex].BitIndex,
                    odCount
                );
            }
            catch
            {
                Error.Show("Please enter a value between 0 and 65535");
                return;
            }
        }
        this.Refresh(this._characterIndex);
        textBox?.SelectAll();
    }

    public void Refresh(int characterIndex)
    {
        this._refreshing = true;
        this._characterIndex = characterIndex;
        var totalOverdrives = OverdriveMode.OverdriveModes.Length;
        var charOffset = this._offsetPartyStats + (this._characterIndex * this._sizePartyMember);
        var offsetLevels =
            (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.OverdriveMode)) + charOffset;
        var offsetFlags =
            (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.OverdriveModes)) + charOffset;
        var offsetCounters =
            (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.OverdriveWarrior)) + charOffset;
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
                    checkLearned.IsChecked = learnedOverdrives[
                        OverdriveMode.OverdriveModes[i].BitIndex
                    ];
                }
                if (dockPanel.Children[1] is TextBox textCount && odCounters != null)
                {
                    textCount.Text = BitConverter
                        .ToUInt16(odCounters, OverdriveMode.OverdriveModes[i].BitIndex * 2)
                        .ToString();
                }
            }
        }

        this._refreshing = false;
    }

    void ButtonMax_Click(object sender, RoutedEventArgs e)
    {
        var charOffset = this._offsetPartyStats + (this._characterIndex * this._sizePartyMember);
        var levelOffset =
            (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.OverdriveLevel)) + charOffset;
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
        var offset =
            (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.OverdriveMode)) + charOffset;
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
