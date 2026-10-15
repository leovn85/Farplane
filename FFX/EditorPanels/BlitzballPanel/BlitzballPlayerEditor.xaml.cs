using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Farplane.Common;
using Farplane.Common.Controls;
using Farplane.Common.Dialogs;
using Farplane.FFX.Data;
using Farplane.FFX.Values;
using MahApps.Metro;
using MahApps.Metro.Controls;

namespace Farplane.FFX.EditorPanels.BlitzballPanel;

/// <summary>
/// Interaction logic for BlitzballPlayerEditor.xaml
/// </summary>
public partial class BlitzballPlayerEditor : UserControl
{
    bool _refresh;
    int _playerIndex = 0;
    BlitzballPlayer[] _players;
    readonly ButtonGrid _buttons = new(3, BlitzballValues.Techs.Length);
    static readonly Tuple<AppTheme, Accent> currentStyle = ThemeManager.DetectAppStyle(
        Application.Current
    );

    readonly Brush _trueBrush = new SolidColorBrush(
        (Color)currentStyle.Item1.Resources["BlackColor"]
    );

    readonly Brush _falseBrush = new SolidColorBrush((Color)currentStyle.Item1.Resources["Gray2"]);

    public BlitzballPlayerEditor()
    {
        this.InitializeComponent();

        foreach (var player in BlitzballValues.Players)
        {
            this.TreeBlitzPlayers.Items.Add(new TreeViewItem { Header = player.Name });
        }

        this.KnownTechs.Content = this._buttons;
        for (var i = 0; i < BlitzballValues.Techs.Length; i++)
        {
            this._buttons.SetContent(i, BlitzballValues.Techs[i].Name);
        }

        this._buttons.ButtonClicked += this.ButtonsOnButtonClicked;
    }

    void ButtonsOnButtonClicked(int buttonIndex)
    {
        var player = this._players[this._playerIndex];
        var skillIndex = BlitzballValues.Techs[buttonIndex].Index;
        var skillData = 0;

        if (buttonIndex < 30)
        {
            // Skill Flags 1
            var byteIndex = (buttonIndex + 1) / 8;
            var bitIndex = (buttonIndex + 1) % 8;
            skillData = player.SkillFlags1;
            var flagBytes = BitConverter.GetBytes(skillData);
            flagBytes[byteIndex] = (byte)(flagBytes[byteIndex] ^ (1 << bitIndex));

            player.SkillFlags1 = BitConverter.ToInt32(flagBytes, 0);
        }
        else
        {
            // Skill Flags 2
            var byteIndex = (buttonIndex - 29) / 8;
            var bitIndex = (buttonIndex - 29) % 8;
            skillData = player.SkillFlags2;
            var flagBytes = BitConverter.GetBytes(skillData);
            flagBytes[byteIndex] = (byte)(flagBytes[byteIndex] ^ (1 << bitIndex));

            player.SkillFlags2 = BitConverter.ToInt32(flagBytes, 0);
        }

        Blitzball.SetPlayerInfo(this._playerIndex, player);

        this.Refresh();
    }

    public void Refresh()
    {
        this._refresh = true;

        if (this.TreeBlitzPlayers.SelectedItem == null)
        {
            (this.TreeBlitzPlayers.Items[0] as TreeViewItem).IsSelected = true;
        }

        // Refresh player data

        this._playerIndex = this.TreeBlitzPlayers.Items.IndexOf(this.TreeBlitzPlayers.SelectedItem);
        this._players = Blitzball.GetPlayers();
        this.RefreshCurrentPlayer();

        this._refresh = false;
    }

    public void RefreshCurrentPlayer()
    {
        this._refresh = true;

        // Refresh player data

        this._playerIndex = this.TreeBlitzPlayers.Items.IndexOf(this.TreeBlitzPlayers.SelectedItem);

        var player = this._players[this._playerIndex];

        this.TextLevel.Text = player.Level.ToString();
        this.TextEXP.Text = player.Experience.ToString();
        this.TextSalary.Text = player.Salary.ToString();

        // Refresh equipped techs
        this.ComboTechCount.SelectedIndex = player.TechCapacity;

        var techData = player.Techs;

        for (var i = 0; i < 5; i++)
        {
            var techButton = (Button)this.FindName("EquippedTech" + (i + 1).ToString().Trim());
            techButton.Visibility =
                i >= player.TechCapacity ? Visibility.Collapsed : Visibility.Visible;

            var tech = BlitzballValues.Techs.FirstOrDefault(t => t.Index == techData[i]);
            if (techData[i] == 0)
            {
                techButton.Content = "< EMPTY >";
            }
            else if (tech == null)
            {
                techButton.Content = "????";
            }
            else
            {
                techButton.Content = tech.Name;
            }
        }

        // Refresh known techs
        for (var i = 0; i < BlitzballValues.Techs.Length; i++)
        {
            var skillIndex = BlitzballValues.Techs[i].Index;
            var skillData = 0;
            if (i < 30)
            {
                // Skill Flags 1
                skillData = player.SkillFlags1;
            }
            else
            {
                // Skill Flags 2
                skillIndex -= 30;
                skillData = player.SkillFlags2;
            }

            var byteIndex = skillIndex / 8;
            var bitIndex = skillIndex % 8;

            var flagBytes = BitConverter.GetBytes(skillData);
            var flagSet = (flagBytes[byteIndex] & (1 << bitIndex)) == (1 << bitIndex);

            if (flagSet)
            {
                // Tech known
                this._buttons[i].Foreground = this._trueBrush;
            }
            else
            {
                // Tech not known
                this._buttons[i].Foreground = this._falseBrush;
            }
        }

        this._refresh = false;
    }

    void TreeBlitzPlayers_OnSelectedItemChanged(
        object sender,
        RoutedPropertyChangedEventArgs<object> e
    )
    {
        if (this._refresh)
        {
            return;
        }

        this.RefreshCurrentPlayer();
    }

    void TechCount_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (this._refresh)
        {
            return;
        }

        var newCount = this.ComboTechCount.SelectedIndex;

        var player = this._players[this._playerIndex];
        player.TechCapacity = (byte)newCount;

        Blitzball.SetPlayerInfo(this._playerIndex, player);

        this.RefreshCurrentPlayer();
    }

    void EquippedTech_OnClick(object sender, RoutedEventArgs e)
    {
        var techIndex = int.Parse((sender as Button).Name.Substring(12)) - 1;
        var techNames = BlitzballValues.Techs.Select(t => t.Name);
        var searchDialog = new SearchDialog(
            techNames.ToList(),
            (sender as Button).Content.ToString()
        )
        {
            Owner = this.TryFindParent<Window>(),
        };
        var success = searchDialog.ShowDialog();

        if (!success.Value)
        {
            return;
        }

        var player = this._players[this._playerIndex];
        if (searchDialog.ResultIndex == -1)
        {
            player.Techs[techIndex] = 0;
        }
        else
        {
            // equip tech
            var tech = BlitzballValues.Techs[searchDialog.ResultIndex];
            player.Techs[techIndex] = (byte)tech.Index;
        }

        Blitzball.SetPlayerInfo(this._playerIndex, player);
        this.RefreshCurrentPlayer();
    }

    void TextLevel_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        var player = this._players[this._playerIndex];

        try
        {
            var level = byte.Parse(this.TextLevel.Text);
            player.Level = level;
            this.TextLevel.SelectAll();
            Blitzball.SetPlayerInfo(this._playerIndex, player);
        }
        catch
        {
            Error.Show("Please enter a value between 0 and 255");
        }

        this.RefreshCurrentPlayer();
    }

    void TextEXP_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        var player = this._players[this._playerIndex];
        try
        {
            var exp = ushort.Parse(this.TextEXP.Text);
            player.Experience = exp;
            this.TextEXP.SelectAll();
            Blitzball.SetPlayerInfo(this._playerIndex, player);
        }
        catch
        {
            Error.Show("Please enter a value between 0 and 65535");
        }

        this.RefreshCurrentPlayer();
    }

    void TextSalary_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        var player = this._players[this._playerIndex];
        try
        {
            var salary = ushort.Parse(this.TextSalary.Text);
            player.Salary = salary;
            this.TextSalary.SelectAll();
            Blitzball.SetPlayerInfo(this._playerIndex, player);
        }
        catch
        {
            Error.Show("Please enter a value between 0 and 65535");
        }

        this.RefreshCurrentPlayer();
    }

    void TextTournamentGoals_OnKeyDown(object sender, KeyEventArgs e) =>
        throw new NotImplementedException();

    void TextLeagueGoals_OnKeyDown(object sender, KeyEventArgs e) =>
        throw new NotImplementedException();
}
