using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Farplane.Common.Dialogs;
using Farplane.FFX2.Values;
using MahApps.Metro.Controls;

namespace Farplane.FFX2.EditorPanels;

/// <summary>
/// Interaction logic for CreatureTrapping.xaml
/// </summary>
public partial class CreatureTrapping : UserControl
{
    byte[] _trapBytes = new byte[60];
    byte[] _podBytes = new byte[9];
    bool _comboShowing = false;
    readonly int _offsetCreatureTrap = (int)OffsetType.CreatureTrapBase;
    readonly int _offsetCreaturePods = (int)OffsetType.CreaturePodBase;

    public CreatureTrapping()
    {
        this.InitializeComponent();
        this.Refresh();
    }

    public void Refresh()
    {
        this._trapBytes = LegacyMemoryReader.ReadBytes(this._offsetCreatureTrap, 60);

        for (var i = 0; i < 15; i++)
        {
            var button = (Button)this.FindName("Trap" + i);
            if (button == null)
            {
                continue;
            }

            var creatureInTrap = this.GetTrap(i);
            if (creatureInTrap is 0 or 0xFF)
            {
                button.Content = "Click to Set Trap";
                continue;
            }
            var creature = Creatures.CreatureList.FirstOrDefault(c => c.ID == creatureInTrap);
            if (creature == null)
            {
                button.Content = $"[{creatureInTrap:X4}] ???? (Unknown ID)";
            }
            else
            {
                button.Content = $"[{creature.ID:X4}] {creature.Name}";
            }
        }

        this._podBytes = LegacyMemoryReader.ReadBytes(this._offsetCreaturePods, 9);
        var trapCount = this._podBytes[0];
        for (var t = 0; t < 8; t++)
        {
            var trapButton = (Button)this.FindName("TrapItem" + t);
            if (trapButton == null)
            {
                continue;
            }

            if (t == trapCount)
            {
                trapButton.Content = "+";
                trapButton.Visibility = Visibility.Visible;
            }
            else if (t > trapCount)
            {
                trapButton.Visibility = Visibility.Collapsed;
            }
            else if (t < 8)
            {
                trapButton.Content = this.GetPodName(t);
                trapButton.Visibility = Visibility.Visible;
            }
            trapButton.Width = 38;
        }
        this._comboShowing = false;
    }

    public string GetPodName(int podIndex)
    {
        return this._podBytes[podIndex + 1] switch
        {
            0x14 => "S",
            0x1E => "M",
            0x32 => "L",
            0x50 => "SP",
            _ => "?",
        };
    }

    public void DeletePod(int podIndex)
    {
        var count = this._podBytes[0];
        var outBytes = new byte[9];
        for (var i = 1; i < podIndex + 1; i++)
        {
            outBytes[i] = this._podBytes[i];
        }

        for (var i = podIndex + 1; i < count; i++)
        {
            if (i + 1 < 9)
            {
                outBytes[i] = this._podBytes[i + 1];
            }
        }

        count--;
        outBytes[0] = count;
        LegacyMemoryReader.WriteBytes(this._offsetCreaturePods, outBytes);
    }

    void TrapButton_RightMouse(object sender, MouseButtonEventArgs e)
    {
        var button = sender as Button;
        var trap = this.GetTrap(int.Parse(button.Name.Substring(4)));
        this.ShowButtonBox(button, trap.ToString("X4"));
    }

    ushort GetTrap(int trapIndex) => BitConverter.ToUInt16(this._trapBytes, trapIndex * 4);

    void SetTrap(int trapIndex, int creatureId)
    {
        LegacyMemoryReader.WriteBytes(
            this._offsetCreatureTrap + (trapIndex * 4),
            BitConverter.GetBytes((ushort)creatureId)
        );
        this.Refresh();
    }

    void ShowButtonBox(Button button, string defaultText)
    {
        this.Refresh();
        var trapId = int.Parse(button.Name.Substring(4));
        var inputBox = new TextBox { Text = defaultText, ContextMenu = null };
        button.Content = inputBox;
        button.KeyDown += (sender, args) =>
        {
            if (args.Key == Key.Escape)
            {
                this.Refresh();
                return;
            }
            if (args.Key != Key.Enter)
            {
                return;
            }

            var parsed = int.TryParse(
                inputBox.Text,
                NumberStyles.HexNumber,
                CultureInfo.InvariantCulture,
                out var newId
            );
            if (parsed)
            {
                this.SetTrap(trapId, newId);
            }
            else
            {
                MessageBox.Show("Please enter a creature ID in hex!");
                return;
            }
        };
        button.UpdateLayout();
        inputBox.SelectionStart = 0;
        inputBox.SelectionLength = inputBox.Text.Length;
        inputBox.Focus();
    }

    void Refresh_Click(object sender, RoutedEventArgs e) => this.Refresh();

    void TrapButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
        {
            return;
        }

        var trapId = int.Parse(button.Name.Substring(4));

        var creatureSearchList = new List<string>();
        foreach (var creature in Creatures.CreatureList)
        {
            creatureSearchList.Add($"{creature.ID:X4} {creature.Name}");
        }

        var creatureSearch = new SearchDialog(creatureSearchList)
        {
            Owner = this.TryFindParent<Window>(),
        };
        creatureSearch.ShowDialog();

        if (creatureSearch.DialogResult == false)
        {
            return;
        }

        var selectedCreature = Creatures.CreatureList[creatureSearch.ResultIndex];

        this.SetTrap(trapId, selectedCreature.ID);
    }

    void TrapItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || this._comboShowing)
        {
            return;
        }

        var itemIndex = int.Parse(button.Name.Substring(8));
        if (itemIndex == this._podBytes[0])
        {
            this.ShowTrapCombo(button);
            return;
        }
        this.DeletePod(itemIndex);
        this.Refresh();
    }

    void ShowTrapCombo(Button button)
    {
        this._comboShowing = true;
        var trapCombo = new ComboBox
        {
            ItemsSource = new string[] { "S", "M", "L", "SP" },
            Width = 48,
            SelectedIndex = 0,
        };

        trapCombo.KeyDown += (sender, args) =>
        {
            switch (args.Key)
            {
                case Key.Enter:
                    if (trapCombo.SelectedIndex >= 0)
                    {
                        var trapID = 0;
                        switch (trapCombo.SelectedIndex)
                        {
                            case 0:
                                trapID = 20;
                                break;
                            case 1:
                                trapID = 30;
                                break;
                            case 2:
                                trapID = 50;
                                break;
                            case 3:
                                trapID = 80;
                                break;
                        }
                        this._podBytes[this._podBytes[0] + 1] = (byte)trapID;
                        this._podBytes[0]++;
                        LegacyMemoryReader.WriteBytes(this._offsetCreaturePods, this._podBytes);
                    }
                    this.Refresh();
                    var nextButtonIndex = this.TrapPanel.Children.IndexOf(button) + 1;

                    if (nextButtonIndex < this.TrapPanel.Children.Count)
                    {
                        var nb = this.TrapPanel.Children[nextButtonIndex];
                        nb?.Focus();
                    }
                    else
                    {
                        button.Focus();
                    }

                    return;
                case Key.Escape:
                    this.Refresh();
                    break;
                default:
                    break;
            }
        };

        button.Content = trapCombo;
        button.Width = 60;
        button.UpdateLayout();
        trapCombo.Focus();
    }
}
