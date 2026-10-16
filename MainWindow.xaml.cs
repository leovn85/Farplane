using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Farplane.Common.Controls;
using Farplane.Common.Dialogs;
using Farplane.FFX;
using Farplane.FFX2;
using Farplane.Memory;
using Farplane.Properties;
using MahApps.Metro;
using MahApps.Metro.Controls;

namespace Farplane;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : MetroWindow
{
    readonly ConfigFlyout _configFlyout = new();

    int _splashCounter = 10;

    public MainWindow()
    {
        var exeVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        if (exeVersion > new Version(Settings.Default.SettingsVersion))
        {
            Settings.Default.Upgrade();
        }

        Settings.Default.SettingsVersion = exeVersion.ToString();
        Settings.Default.Save();

        try
        {
            // Load app theme and accent
            var currentTheme = ThemeManager.GetAppTheme(Settings.Default.AppTheme);
            var currentAccent = ThemeManager.GetAccent(Settings.Default.AppAccent);

            ThemeManager.ChangeAppStyle(Application.Current, currentAccent, currentTheme);
        }
        catch
        {
            // Theme error, revert to default
            Settings.Default.AppTheme = "BaseLight";
            Settings.Default.AppAccent = "Blue";

            Settings.Default.Save();

            var currentTheme = ThemeManager.GetAppTheme(Settings.Default.AppTheme);
            var currentAccent = ThemeManager.GetAccent(Settings.Default.AppAccent);

            ThemeManager.ChangeAppStyle(Application.Current, currentAccent, currentTheme);
        }

        this.InitializeComponent();
        var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        this.Title = string.Format(this.Title, $"{version.Major}.{version.Minor}.{version.Build}");

        this.Flyouts = new FlyoutsControl();
        this.Flyouts.Items.Add(this._configFlyout);

        // Auto-detect running game on startup
        this.Loaded += (s, e) => TryAutoAttach();
    }

    void TryAutoAttach()
    {
        // Check for FFX first, then FFX-2
        var ffxProcess = Process.GetProcesses()
            .FirstOrDefault(p =>
                string.Equals(p.ProcessName, "FFX", StringComparison.OrdinalIgnoreCase)
            );

        if (ffxProcess != null)
        {
            try
            {
                if (GameMemory.Attach(ffxProcess))
                {
                    LegacyMemoryReader.Attach(ffxProcess);
                    this.Hide();
                    var editor = new FFXEditor();
                    editor.ShowDialog();
                    GameMemory.Detach();
                    if (Settings.Default.CloseWithGame)
                    {
                        Environment.Exit(0);
                    }
                    this.Show();
                    return;
                }
            }
            catch { }
        }

        var ffx2Process = Process.GetProcesses()
            .FirstOrDefault(p =>
                string.Equals(p.ProcessName, "FFX-2", StringComparison.OrdinalIgnoreCase)
            );

        if (ffx2Process != null)
        {
            try
            {
                if (GameMemory.Attach(ffx2Process))
                {
                    LegacyMemoryReader.Attach(ffx2Process);
                    this.Hide();
                    var editor = new FFX2Editor();
                    editor.ShowDialog();
                    GameMemory.Detach();
                    if (Settings.Default.CloseWithGame)
                    {
                        Environment.Exit(0);
                    }
                    this.Show();
                    return;
                }
            }
            catch { }
        }
    }

    void FFX2_Click(object sender, RoutedEventArgs e)
    {
        this._configFlyout.IsOpen = false;
        var processSelect = new ProcessSelectWindow("FFX-2") { Owner = this };
        processSelect.ShowDialog();

        if (processSelect.DialogResult == true)
        {
            this.Hide();
            var FFX2Editor = new FFX2Editor();
            FFX2Editor.ShowDialog();
            GameMemory.Detach();
            if (Settings.Default.CloseWithGame)
            {
                Environment.Exit(0);
            }

            this.Show();
            this.Topmost = true;
            this.Topmost = false;
        }
    }

    void FFX_Click(object sender, RoutedEventArgs e)
    {
        this._configFlyout.IsOpen = false;
        var processSelect = new ProcessSelectWindow("FFX") { Owner = this };
        processSelect.ShowDialog();

        if (processSelect.DialogResult == true)
        {
            this.Hide();

            if (processSelect.ResultProcess == null)
            {
                try
                {
                    var FFXEditor = new FFXEditor(true);
                    FFXEditor.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error running FFXEditor:\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                                    "Start Editor Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
            }
            else
            {
                try
                {
                    var FFXEditor = new FFXEditor();
                    FFXEditor.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error running FFXEditor:\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                                    "Start Editor Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
            }
            GameMemory.Detach();
            if (Settings.Default.CloseWithGame)
            {
                Environment.Exit(0);
            }

            this.Show();
            this.Topmost = true;
            this.Topmost = false;
        }
    }

    void ButtonConfig_Click(object sender, RoutedEventArgs e) =>
        this._configFlyout.IsOpen = !this._configFlyout.IsOpen;

    void SplashLogo_MouseDown(object sender, MouseButtonEventArgs e)
    {
        this._splashCounter--;
        if (this._splashCounter > 0)
        {
            return;
        }

        this._splashCounter = 10;
        var credits = new CreditsWindow
        {
            Owner = this,
            ShowInTaskbar = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
        };
        credits.ShowDialog();
    }
}
