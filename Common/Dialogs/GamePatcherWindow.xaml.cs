using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Farplane.Common.Patcher;
using MahApps.Metro.Controls;
using Microsoft.Win32;

namespace Farplane.Common.Dialogs;

public partial class GamePatcherWindow : MetroWindow
{
    string _gameDir = string.Empty;
    readonly SolidColorBrush _greenBrush = new(Colors.ForestGreen);
    readonly SolidColorBrush _grayBrush = new(Colors.Gray);
    readonly SolidColorBrush _orangeBrush = new(Colors.DarkOrange);

    public GamePatcherWindow()
    {
        this.InitializeComponent();
        this.Loaded += (s, e) =>
        {
            this._gameDir = GamePatcher.DetectGameDirectory();
            this.TextGamePath.Text = string.IsNullOrEmpty(this._gameDir)
                ? "Game directory not found. Please click Browse..."
                : this._gameDir;
            this.RefreshAllStatuses();
        };
    }

    void ButtonBrowse_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select FFX.exe in your game directory",
            Filter = "FFX Executable (FFX.exe)|FFX.exe|All Executables (*.exe)|*.exe"
        };
        if (dialog.ShowDialog(this) == true)
        {
            var folder = Path.GetDirectoryName(dialog.FileName);
            if (!string.IsNullOrEmpty(folder))
            {
                this._gameDir = folder;
                this.TextGamePath.Text = this._gameDir;
                this.RefreshAllStatuses();
            }
        }
    }

    void ButtonAutoDetect_Click(object sender, RoutedEventArgs e)
    {
        var detected = GamePatcher.DetectGameDirectory();
        if (!string.IsNullOrEmpty(detected))
        {
            this._gameDir = detected;
            this.TextGamePath.Text = this._gameDir;
            this.RefreshAllStatuses();
            MessageBox.Show(this, $"Detected game folder:\n{detected}", "Game Detected", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show(this, "Could not automatically locate Final Fantasy X/X-2 installation folder. Please use Browse.", "Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    void RefreshAllStatuses()
    {
        if (string.IsNullOrEmpty(this._gameDir) || !Directory.Exists(this._gameDir))
        {
            this.SetUnknownStatuses();
            return;
        }

        var ffx = GamePatcher.GetStatus(this._gameDir, GamePatcher.FfxExe);
        var ffx2 = GamePatcher.GetStatus(this._gameDir, GamePatcher.Ffx2Exe);
        var will = GamePatcher.GetStatus(this._gameDir, GamePatcher.FfxWillExe);

        // 1. 4GB Patch Status
        this.Check4GB_FFX.IsEnabled = ffx.Exists;
        this.Check4GB_FFX.IsChecked = ffx.Is4GBEnabled;
        this.Status4GB_FFX.Text = ffx.Exists ? (ffx.Is4GBEnabled ? "4GB Active" : "Original 2GB") : "Not Found";
        this.Status4GB_FFX.Foreground = ffx.Is4GBEnabled ? this._greenBrush : this._grayBrush;

        this.Check4GB_FFX2.IsEnabled = ffx2.Exists;
        this.Check4GB_FFX2.IsChecked = ffx2.Is4GBEnabled;
        this.Status4GB_FFX2.Text = ffx2.Exists ? (ffx2.Is4GBEnabled ? "4GB Active" : "Original 2GB") : "Not Found";
        this.Status4GB_FFX2.Foreground = ffx2.Is4GBEnabled ? this._greenBrush : this._grayBrush;

        // 2. Farplane Fix Status
        this.CheckFarplane_FFX.IsEnabled = ffx.Exists && ffx.IsFarplaneFixed.HasValue;
        if (ffx.IsFarplaneFixed.HasValue)
        {
            this.CheckFarplane_FFX.IsChecked = ffx.IsFarplaneFixed.Value;
            this.StatusFarplane_FFX.Text = ffx.IsFarplaneFixed.Value ? "Video Fixed (v1.2)" : "Default Grey";
            this.StatusFarplane_FFX.Foreground = ffx.IsFarplaneFixed.Value ? this._greenBrush : this._grayBrush;
        }
        else
        {
            this.CheckFarplane_FFX.IsChecked = false;
            this.StatusFarplane_FFX.Text = ffx.Exists ? "Custom Build" : "Not Found";
            this.StatusFarplane_FFX.Foreground = this._orangeBrush;
        }

        // 3. Audio Undub Status
        this.CheckUndub_FFX.IsEnabled = ffx.Exists;
        this.CheckUndub_FFX.IsChecked = ffx.IsAudioUndubbed;
        this.StatusUndub_FFX.Text = ffx.Exists ? (ffx.IsAudioUndubbed ? "Japanese" : "English") : "Not Found";
        this.StatusUndub_FFX.Foreground = ffx.IsAudioUndubbed ? this._greenBrush : this._grayBrush;

        this.CheckUndub_FFX2.IsEnabled = ffx2.Exists;
        this.CheckUndub_FFX2.IsChecked = ffx2.IsAudioUndubbed;
        this.StatusUndub_FFX2.Text = ffx2.Exists ? (ffx2.IsAudioUndubbed ? "Japanese" : "English") : "Not Found";
        this.StatusUndub_FFX2.Foreground = ffx2.IsAudioUndubbed ? this._greenBrush : this._grayBrush;

        this.CheckUndub_Will.IsEnabled = will.Exists;
        this.CheckUndub_Will.IsChecked = will.IsAudioUndubbed;
        this.StatusUndub_Will.Text = will.Exists ? (will.IsAudioUndubbed ? "Japanese" : "English") : "Not Found";
        this.StatusUndub_Will.Foreground = will.IsAudioUndubbed ? this._greenBrush : this._grayBrush;
    }

    void SetUnknownStatuses()
    {
        this.Status4GB_FFX.Text = "[Unknown]";
        this.Status4GB_FFX2.Text = "[Unknown]";
        this.StatusFarplane_FFX.Text = "[Unknown]";
        this.StatusUndub_FFX.Text = "[Unknown]";
        this.StatusUndub_FFX2.Text = "[Unknown]";
        this.StatusUndub_Will.Text = "[Unknown]";
    }

    void ButtonApply_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(this._gameDir) || !Directory.Exists(this._gameDir))
        {
            MessageBox.Show(this, "Please select a valid game folder first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var ffxPath = Path.Combine(this._gameDir, GamePatcher.FfxExe);
        var ffx2Path = Path.Combine(this._gameDir, GamePatcher.Ffx2Exe);
        var willPath = Path.Combine(this._gameDir, GamePatcher.FfxWillExe);

        try
        {
            int appliedCount = 0;

            // 1. 4GB Patch
            if (File.Exists(ffxPath))
            {
                bool target = this.Check4GB_FFX.IsChecked == true;
                if (GamePatcher.IsLargeAddressAware(ffxPath) != target)
                {
                    GamePatcher.SetLargeAddressAware(ffxPath, target);
                    appliedCount++;
                }
            }
            if (File.Exists(ffx2Path))
            {
                bool target = this.Check4GB_FFX2.IsChecked == true;
                if (GamePatcher.IsLargeAddressAware(ffx2Path) != target)
                {
                    GamePatcher.SetLargeAddressAware(ffx2Path, target);
                    appliedCount++;
                }
            }

            // 2. Farplane Fix (Embedded 11-part v1.2 patch)
            if (File.Exists(ffxPath))
            {
                bool target = this.CheckFarplane_FFX.IsChecked == true;
                bool? current = GamePatcher.IsFarplaneFixed(ffxPath);
                if (current.HasValue && current.Value != target)
                {
                    if (GamePatcher.ApplyFarplaneFix(ffxPath, undo: !target, out string fixMsg))
                    {
                        appliedCount++;
                    }
                    else
                    {
                        MessageBox.Show(this, $"Farplane Fix error:\n{fixMsg}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }

            // 3. Audio Undub Mod (Supports both Apply and Undo)
            if (File.Exists(ffxPath))
            {
                bool target = this.CheckUndub_FFX.IsChecked == true;
                if (GamePatcher.IsAudioUndubbed(ffxPath) != target)
                {
                    if (GamePatcher.ApplyUndubPatch(ffxPath, undo: !target) > 0)
                        appliedCount++;
                }
            }
            if (File.Exists(ffx2Path))
            {
                bool target = this.CheckUndub_FFX2.IsChecked == true;
                if (GamePatcher.IsAudioUndubbed(ffx2Path) != target)
                {
                    if (GamePatcher.ApplyUndubPatch(ffx2Path, undo: !target) > 0)
                        appliedCount++;
                }
            }
            if (File.Exists(willPath))
            {
                bool target = this.CheckUndub_Will.IsChecked == true;
                if (GamePatcher.IsAudioUndubbed(willPath) != target)
                {
                    if (GamePatcher.ApplyUndubPatch(willPath, undo: !target) > 0)
                        appliedCount++;
                }
            }

            this.RefreshAllStatuses();
            MessageBox.Show(
                this,
                $"Operation completed successfully!\nModified {appliedCount} configuration items.\nOriginal backups are preserved as .original files.",
                "Patches Applied",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                this,
                $"Failed to apply patches:\n{ex.Message}\n\nMake sure the game is not currently running.",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }

    void ButtonRestore_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(this._gameDir) || !Directory.Exists(this._gameDir))
        {
            return;
        }

        var result = MessageBox.Show(
            this,
            "Restore all executables (FFX.exe, FFX-2.exe, Will) to their clean, original state?",
            "Confirm Restore",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question
        );
        if (result != MessageBoxResult.Yes)
            return;

        string[] exes = [GamePatcher.FfxExe, GamePatcher.Ffx2Exe, GamePatcher.FfxWillExe];
        int restored = 0;
        foreach (var exe in exes)
        {
            var p = Path.Combine(this._gameDir, exe);
            if (GamePatcher.RestoreFromOriginal(p, out _))
            {
                restored++;
            }
        }

        this.RefreshAllStatuses();
        MessageBox.Show(this, $"Restored {restored} files back to clean state.", "Restore Completed", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    void ButtonRefresh_Click(object sender, RoutedEventArgs e) => this.RefreshAllStatuses();
    void ButtonClose_Click(object sender, RoutedEventArgs e) => this.Close();
}
