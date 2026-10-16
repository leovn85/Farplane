using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Farplane.Common;
using Farplane.FFX.Data;
using Farplane.FFX.Values;
using Farplane.Memory;

namespace Farplane.FFX.EditorPanels.GeneralPanel;

public partial class GeneralPanel : UserControl
{
    readonly Character[] _enumValues = (Character[])Enum.GetValues(typeof(Character));
    bool _refreshing;
    readonly DispatcherTimer _chocoboTimer;

    public GeneralPanel()
    {
        this.InitializeComponent();
        this._refreshing = true;

        var characterList = Enum.GetNames(typeof(Character));
        foreach (ComboBox comboBox in this.StackCurrentParty.Children)
        {
            comboBox.ItemsSource = characterList;
        }

        this._chocoboTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(200)
        };
        this._chocoboTimer.Tick += this.ChocoboTimer_Tick;

        this._refreshing = false;
    }

    public void Refresh()
    {
        if (!GameMemory.IsAttached)
            return;

        this._refreshing = true;
        try
        {
            // Overview
            this.TextGil.Text = Data.General.CurrentGil.ToString();
            this.TextTidusOverdrive.Text = Data.General.TidusOverdrive.ToString();
            this.TextTotalBattles.Text = Data.General.TotalBattles.ToString();
            this.TextStoryProgress.Text = Data.General.StoryProgress.ToString();
            this.TextRikkuSteals.Text = Data.General.SuccessfulRikkuSteals.ToString();
            this.TextBribeGil.Text = Data.General.BribeGilSpent.ToString();
            this.TextYojimboCompat.Text = Data.General.YojimboCompatibility.ToString();
            this.CheckRikkuCostume.IsChecked = Data.General.RikkuAlBhedCostume;

            // Current Party
            var partyList = Data.Party.GetActiveParty();
            for (var i = 0; i < 8; i++)
            {
                var comboBox = this.StackCurrentParty.Children[i] as ComboBox;
                if (comboBox != null)
                {
                    comboBox.SelectedIndex = Array.IndexOf(this._enumValues, partyList[i]);
                }
            }

            // Minigames: Lightning
            this.TextLightningAll.Text = Data.General.LightningTotalAll.ToString();
            this.TextLightningDodged.Text = Data.General.LightningTotalDodged.ToString();
            this.TextLightningStreak.Text = Data.General.LightningRecordStreak.ToString();

            // Luca Theater
            this.TextMusicSpheres.Text = Data.General.MusicSpheres.ToString();
            this.TextMovieSpheres.Text = Data.General.MovieSpheres.ToString();

            // Temple Seals
            this.CheckSeal0.IsChecked = Data.General.GetTempleSeal(0);
            this.CheckSeal1.IsChecked = Data.General.GetTempleSeal(1);
            this.CheckSeal2.IsChecked = Data.General.GetTempleSeal(2);
            this.CheckSeal3.IsChecked = Data.General.GetTempleSeal(3);
            this.CheckSeal4.IsChecked = Data.General.GetTempleSeal(4);
            this.CheckSeal5.IsChecked = Data.General.GetTempleSeal(5);
            this.CheckSeal6.IsChecked = Data.General.GetTempleSeal(6);

            // Affection (Characters 1 to 6)
            this.TextAffection1.Text = Data.General.GetAffection(1).ToString();
            this.TextAffection2.Text = Data.General.GetAffection(2).ToString();
            this.TextAffection3.Text = Data.General.GetAffection(3).ToString();
            this.TextAffection4.Text = Data.General.GetAffection(4).ToString();
            this.TextAffection5.Text = Data.General.GetAffection(5).ToString();
            this.TextAffection6.Text = Data.General.GetAffection(6).ToString();
        }
        finally
        {
            this._refreshing = false;
        }
    }

    void TextGil_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;
        try
        {
            Data.General.CurrentGil = int.Parse(this.TextGil.Text);
            this.TextGil.SelectAll();
        }
        catch { Error.Show("The value you entered was invalid."); }
    }

    void ButtonMaxGil_Click(object sender, RoutedEventArgs e)
    {
        Data.General.CurrentGil = 999999999;
        this.Refresh();
    }

    void TextTidusOverdrive_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;
        try
        {
            Data.General.TidusOverdrive = int.Parse(this.TextTidusOverdrive.Text);
            this.TextTidusOverdrive.SelectAll();
        }
        catch { Error.Show("The value you entered was invalid."); }
    }

    void TextTotalBattles_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;
        try
        {
            Data.General.TotalBattles = uint.Parse(this.TextTotalBattles.Text);
            this.TextTotalBattles.SelectAll();
        }
        catch { Error.Show("The value you entered was invalid."); }
    }

    void TextStoryProgress_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;
        try
        {
            Data.General.StoryProgress = ushort.Parse(this.TextStoryProgress.Text);
            this.TextStoryProgress.SelectAll();
        }
        catch { Error.Show("The value you entered was invalid."); }
    }

    void TextRikkuSteals_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;
        try
        {
            Data.General.SuccessfulRikkuSteals = uint.Parse(this.TextRikkuSteals.Text);
            this.TextRikkuSteals.SelectAll();
        }
        catch { Error.Show("The value you entered was invalid."); }
    }

    void TextBribeGil_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;
        try
        {
            Data.General.BribeGilSpent = uint.Parse(this.TextBribeGil.Text);
            this.TextBribeGil.SelectAll();
        }
        catch { Error.Show("The value you entered was invalid."); }
    }

    void TextYojimboCompat_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;
        try
        {
            Data.General.YojimboCompatibility = uint.Parse(this.TextYojimboCompat.Text);
            this.TextYojimboCompat.SelectAll();
        }
        catch { Error.Show("Please enter a value between 0 and 255."); }
    }

    void CheckRikkuCostume_Changed(object sender, RoutedEventArgs e)
    {
        if (this._refreshing)
            return;
        Data.General.RikkuAlBhedCostume = this.CheckRikkuCostume.IsChecked == true;
    }

    void PartyMember_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (this._refreshing)
            return;
        var charArray = new Character[8];
        for (var i = 0; i < 8; i++)
        {
            var comboBox = this.StackCurrentParty.Children[i] as ComboBox;
            charArray[i] = this._enumValues[comboBox.SelectedIndex];
        }
        Data.Party.SetActiveParty(charArray);
    }

    // --- Minigames Handlers ---
    void TextLightningAll_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;
        try
        { Data.General.LightningTotalAll = ushort.Parse(this.TextLightningAll.Text); this.TextLightningAll.SelectAll(); }
        catch { Error.Show("Invalid number."); }
    }

    void TextLightningDodged_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;
        try
        { Data.General.LightningTotalDodged = ushort.Parse(this.TextLightningDodged.Text); this.TextLightningDodged.SelectAll(); }
        catch { Error.Show("Invalid number."); }
    }

    void TextLightningStreak_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;
        try
        { Data.General.LightningRecordStreak = ushort.Parse(this.TextLightningStreak.Text); this.TextLightningStreak.SelectAll(); }
        catch { Error.Show("Invalid number."); }
    }

    void ButtonCompleteLightning_Click(object sender, RoutedEventArgs e)
    {
        Data.General.Complete200LightningDodges();
        this.Refresh();
        MessageBox.Show(
            "Lightning streak set to 200!\n\nDodge one more lightning bolt in the Thunder Plains to unlock the reward chest in the Travel Agency.",
            "Venus Sigil Ready",
            MessageBoxButton.OK,
            MessageBoxImage.Information
        );
    }

    void ButtonWinChocobo_Click(object sender, RoutedEventArgs e)
    {
        Data.General.WinChocoboRace();
        MessageBox.Show(
            "Race parameters set to 0:0.0 with 50 Balloons collected and 0 Hits!\nCross the finish line to claim the Sun Sigil.",
            "Chocobo Catcher",
            MessageBoxButton.OK,
            MessageBoxImage.Information
        );
    }

    void CheckFreezeChocoboTimer_Changed(object sender, RoutedEventArgs e)
    {
        if (this.CheckFreezeChocoboTimer.IsChecked == true)
            this._chocoboTimer.Start();
        else
            this._chocoboTimer.Stop();
    }

    void ChocoboTimer_Tick(object sender, EventArgs e)
    {
        if (GameMemory.IsAttached)
        {
            Data.General.FreezeChocoboTimer();
        }
    }

    void ButtonFastWinBlitzball_Click(object sender, RoutedEventArgs e)
    {
        Data.General.FastWinBlitzballMatch(true);
        MessageBox.Show(
            "Goal added and timer advanced to 5:00 to conclude the match immediately!",
            "Blitzball Win Helper",
            MessageBoxButton.OK,
            MessageBoxImage.Information
        );
    }
    void TextMusicSpheres_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;
        try
        { Data.General.MusicSpheres = byte.Parse(this.TextMusicSpheres.Text); this.TextMusicSpheres.SelectAll(); }
        catch { Error.Show("Please enter a value between 0 and 71."); }
    }
    void TextMovieSpheres_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;
        try
        { Data.General.MovieSpheres = byte.Parse(this.TextMovieSpheres.Text); this.TextMovieSpheres.SelectAll(); }
        catch { Error.Show("Please enter a value between 0 and 50."); }
    }
    void ButtonMaxMusic_Click(object sender, RoutedEventArgs e)
    {
        Data.General.UnlockAllMusicSpheres();
        this.Refresh();
    }
    void ButtonMaxMovie_Click(object sender, RoutedEventArgs e)
    {
        Data.General.UnlockAllMovieSpheres();
        this.Refresh();
    }
    void ButtonUnlockAllMusic_Click(object sender, RoutedEventArgs e)
    {
        Data.General.UnlockAllMusicSpheres();
        this.Refresh();
        MessageBox.Show("All 71 Music Spheres unlocked at Luca Theater!", "Music Unlocked", MessageBoxButton.OK, MessageBoxImage.Information);
    }
    void ButtonUnlockAllMovie_Click(object sender, RoutedEventArgs e)
    {
        Data.General.UnlockAllMovieSpheres();
        this.Refresh();
        MessageBox.Show("All 50 Movie Spheres unlocked at Luca Theater!", "Movies Unlocked", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    // --- Temple Seals Handlers ---
    void SealCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (this._refreshing)
            return;
        if (sender is CheckBox checkBox && checkBox.Tag is string tagStr && int.TryParse(tagStr, out var sealIndex))
        {
            Data.General.SetTempleSeal(sealIndex, checkBox.IsChecked == true);
        }
    }

    void ButtonUnlockAllSeals_Click(object sender, RoutedEventArgs e)
    {
        Data.General.UnlockAllTempleSeals();
        this.Refresh();
        MessageBox.Show(
            "All 7 Temple Seals unlocked!\nYou can now enter the chamber at Baaj Temple to acquire Anima.",
            "Temple Seals Unlocked",
            MessageBoxButton.OK,
            MessageBoxImage.Information
        );
    }

    // --- Affection Handlers ---
    void TextAffection_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;
        if (sender is TextBox textBox && textBox.Tag is string tagStr && int.TryParse(tagStr, out var charIndex))
        {
            try
            {
                var val = uint.Parse(textBox.Text);
                Data.General.SetAffection(charIndex, val);
                textBox.SelectAll();
            }
            catch { Error.Show("Invalid number."); }
        }
    }
}
