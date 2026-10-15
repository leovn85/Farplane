using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Farplane.Properties;
using MahApps.Metro;
using MahApps.Metro.Controls;

namespace Farplane.Common.Controls;

/// <summary>
/// Interaction logic for ConfigFlyout.xaml
/// </summary>
public partial class ConfigFlyout : Flyout
{
    readonly bool canSetTheme = false;

    public ConfigFlyout()
    {
        this.InitializeComponent();

        this.ComboTheme.ItemsSource = ThemeManager.AppThemes;
        this.ComboAccent.ItemsSource = ThemeManager.Accents;

        var currentTheme = ThemeManager.GetAppTheme(Settings.Default.AppTheme);
        var currentAccent = ThemeManager.GetAccent(Settings.Default.AppAccent);

        this.ComboTheme.SelectedIndex = ThemeManager.AppThemes.ToList().IndexOf(currentTheme);

        this.ComboAccent.SelectedIndex = ThemeManager.Accents.ToList().IndexOf(currentAccent);

        this.CheckNeverShowUnXWarning.IsChecked = Settings.Default.NeverShowUnXWarning;
        this.CheckCloseWithGame.IsChecked = Settings.Default.CloseWithGame;
        this.CheckShowAllProcesses.IsChecked = Settings.Default.ShowAllProcesses;

        this.canSetTheme = true;
    }

    void ComboAccent_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!this.canSetTheme)
        {
            return;
        }

        ThemeManager.ChangeAppStyle(
            Application.Current,
            (Accent)this.ComboAccent.SelectedItem,
            (AppTheme)this.ComboTheme.SelectedItem
        );
        this.SettingUpdated(sender, e);
    }

    void SettingUpdated(object sender, RoutedEventArgs e)
    {
        if (!this.canSetTheme)
        {
            return;
        }

        Settings.Default.NeverShowUnXWarning = this.CheckNeverShowUnXWarning.IsChecked.Value;
        Settings.Default.CloseWithGame = this.CheckCloseWithGame.IsChecked.Value;
        Settings.Default.ShowAllProcesses = this.CheckShowAllProcesses.IsChecked.Value;
        Settings.Default.AppAccent = (this.ComboAccent.SelectedItem as Accent).Name;
        Settings.Default.AppTheme = (this.ComboTheme.SelectedItem as AppTheme).Name;
        Settings.Default.Save();
    }
}
