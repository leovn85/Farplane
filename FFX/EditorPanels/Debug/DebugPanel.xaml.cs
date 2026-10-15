using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Farplane.Memory;

namespace Farplane.FFX.EditorPanels.Debug;

public partial class DebugPanel : UserControl
{
    int _debugOffset => OffsetScanner.GetOffset(GameOffset.FFX_DebugFlags);
    List<int> known;
    List<int> unknown;

    public DebugPanel() => this.InitializeComponent();

    public void Refresh()
    {
        var array = GameMemory.Read<byte>(
            this._debugOffset,
            (int)DebugFlags.Count + 1,
            isRelative: false
        );
        if (array == null)
            return;

        var names = Enum.GetNames(typeof(DebugFlags));
        var values = Enum.GetValues(typeof(DebugFlags));
        this.known = [];
        this.unknown = [];
        for (var i = 0; i < names.Length; i++)
        {
            if (names[i].StartsWith("Unknown"))
            {
                this.unknown.Add((int)values.GetValue(i));
            }
            else
            {
                this.known.Add((int)values.GetValue(i));
            }
        }
        for (var j = 0; j < this.known.Count; j++)
        {
            if (
                this.StackDebugOptions.Children.OfType<CheckBox>().ElementAtOrDefault(j) is
                { } checkBox && this.known[j] < array.Length
            )
            {
                checkBox.IsChecked = array[this.known[j]] != 0;
            }
        }
        for (var k = 0; k < this.unknown.Count; k++)
        {
            if (this.StackUnknown.Children.OfType<CheckBox>().ElementAtOrDefault(k) is { } checkBox
                && this.unknown[k] < array.Length)
            {
                checkBox.IsChecked = array[this.unknown[k]] != 0;
            }
        }
    }

    void CheckBox_Changed(object sender, RoutedEventArgs e)
    {
        for (var i = 0; i < this.known.Count; i++)
        {
            GameMemory.Write(
                this._debugOffset + this.known[i],
                (byte)((this.StackDebugOptions.Children[i] as CheckBox).IsChecked.Value ? 1 : 0),
                isRelative: false
            );
        }
        for (var j = 0; j < this.unknown.Count; j++)
        {
            GameMemory.Write(
                this._debugOffset + this.unknown[j],
                (byte)((this.StackUnknown.Children[j] as CheckBox).IsChecked.Value ? 1 : 0),
                isRelative: false
            );
        }
        this.Refresh();
    }

    void CheckShowUnknownFlags_OnChecked(object sender, RoutedEventArgs e)
    {
        this.StackUnknown.Visibility =
            (!this.CheckShowUnknownFlags.IsChecked.Value) ? Visibility.Hidden : Visibility.Visible;
        this.TextUnknownWarning.Visibility =
            (!this.CheckShowUnknownFlags.IsChecked.Value)
                ? Visibility.Collapsed
                : Visibility.Visible;
    }
}
