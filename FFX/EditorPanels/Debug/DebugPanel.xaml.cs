using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Farplane.FFX.Values;
using Farplane.Memory;

namespace Farplane.FFX.EditorPanels.Debug;

public partial class DebugPanel : UserControl
{
    int _debugOffset => OffsetScanner.GetOffset(GameOffset.FFX_DebugFlags);
    int _battlePauseOffset => OffsetScanner.GetOffset(GameOffset.FFX_BattlePause);
    bool _refreshing;

    public DebugPanel()
    {
        this.InitializeComponent();
    }

    public void Refresh()
    {
        if (!GameMemory.IsAttached)
        {
            return;
        }

        var debugOffset = this._debugOffset;
        if (debugOffset == 0)
        {
            return;
        }

        var array = GameMemory.Read<byte>(debugOffset, (int)DebugFlags.Count, isRelative: false);
        if (array == null || array.Length < (int)DebugFlags.Count)
        {
            return;
        }

        this._refreshing = true;
        try
        {
            // Read True Battle Pause [pause] at FFX_BtlBase + 0x11
            var pauseOffset = this._battlePauseOffset;
            if (pauseOffset != 0)
            {
                var pauseVal = GameMemory.Read<byte>(pauseOffset, isRelative: false);
                this.CheckBattlePause.IsChecked = pauseVal != 0;
            }

            // Sync all tag-based debug check boxes
            var checkBoxes = this.FindChildren<CheckBox>();
            foreach (var checkBox in checkBoxes)
            {
                if (checkBox.Tag is string tagName && Enum.TryParse<DebugFlags>(tagName, out var flag))
                {
                    var flagIndex = (int)flag;
                    if (flagIndex < array.Length)
                    {
                        checkBox.IsChecked = array[flagIndex] != 0;
                    }
                }
            }

            // Read Forced Ambush State at FFX_DebugFlags + 0x2B
            var ambushByte = array[(int)DebugFlags.ForcedAmbushState];
            this.ComboAmbushState.SelectedIndex = ambushByte switch
            {
                0x00 => 1,
                0x01 => 2,
                0x02 => 3,
                _ => 0, // 0xFF or any default
            };
        }
        finally
        {
            this._refreshing = false;
        }
    }

    void CheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (this._refreshing || !GameMemory.IsAttached)
        {
            return;
        }

        if (sender is not CheckBox checkBox || checkBox.Tag is not string tagName)
        {
            return;
        }

        if (Enum.TryParse<DebugFlags>(tagName, out var flag))
        {
            var flagOffset = this._debugOffset + (int)flag;
            var flagVal = (byte)(checkBox.IsChecked == true ? 1 : 0);
            GameMemory.Write(flagOffset, flagVal, isRelative: false);
        }
    }

    void CheckBattlePause_Changed(object sender, RoutedEventArgs e)
    {
        if (this._refreshing || !GameMemory.IsAttached)
        {
            return;
        }

        var pauseOffset = this._battlePauseOffset;
        if (pauseOffset == 0)
        {
            return;
        }

        var pauseVal = (byte)(this.CheckBattlePause.IsChecked == true ? 1 : 0);
        GameMemory.Write(pauseOffset, pauseVal, isRelative: false);
    }

    void ComboAmbushState_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (this._refreshing || !GameMemory.IsAttached)
        {
            return;
        }

        var flagOffset = this._debugOffset + (int)DebugFlags.ForcedAmbushState;
        byte ambushVal = this.ComboAmbushState.SelectedIndex switch
        {
            1 => 0x00, // Normal
            2 => 0x01, // Preemptive Strike
            3 => 0x02, // Ambushed
            _ => 0xFF, // Default / Random
        };

        GameMemory.Write(flagOffset, ambushVal, isRelative: false);
    }

    // Helper extension to find all children recursively in WPF visual tree
    private System.Collections.Generic.IEnumerable<T> FindChildren<T>() where T : DependencyObject
    {
        return FindVisualChildren<T>(this);
    }

    private static System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
    {
        if (depObj == null)
            yield break;

        for (var i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(depObj); i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(depObj, i);
            if (child is T matchingChild)
            {
                yield return matchingChild;
            }

            foreach (var childOfChild in FindVisualChildren<T>(child))
            {
                yield return childOfChild;
            }
        }
    }
}
