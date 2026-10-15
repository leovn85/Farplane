using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Farplane.Common.Dialogs;
using Farplane.FFX.Data;
using Farplane.FFX.Values;
using MahApps.Metro.Controls;

namespace Farplane.FFX.EditorPanels.BlitzballPanel;

/// <summary>
/// Interaction logic for BlitzballGeneralEditor.xaml
/// </summary>
public partial class BlitzballLeagueEditor : UserControl
{
    bool _refresh;

    public BlitzballLeagueEditor()
    {
        this.InitializeComponent();

        this._refresh = true;
        this.ComboLeagueStatus.ItemsSource = BlitzballValues.LeagueStates.Select(state =>
            state.Name
        );
        foreach (
            var teamCombo in this.FindChildren<ComboBox>()
                .Where(child => child.Name.StartsWith("LeagueTeam"))
        )
        {
            teamCombo.ItemsSource = BlitzballValues.Teams.Select(team => team.Name);
        }

        this._refresh = false;
    }

    public void Refresh()
    {
        this._refresh = true;
        var prizes = BlitzballValues.Prizes;
        var blitzData = Blitzball.ReadBlitzballData();

        // Update League status
        var leagueStatusIndex = Array.IndexOf(
            BlitzballValues.LeagueStates,
            BlitzballValues.LeagueStates.First(state => state.Index == blitzData.LeagueStatus)
        );

        this.ComboLeagueStatus.SelectedIndex = leagueStatusIndex;

        // Update team matchups
        for (var i = 0; i < 6; i++)
        {
            if (this.FindName($"LeagueTeam{i}") is not ComboBox teamCombo)
            {
                continue;
            }

            teamCombo.SelectedIndex = blitzData.LeagueMatchups[i];
        }

        // Update prizes
        for (var i = 0; i < 8; i++)
        {
            if (this.FindName($"Prize{i}") is not Button prizeButton)
            {
                continue;
            }

            var currentPrize = prizes.FirstOrDefault(prize =>
                prize.Index == blitzData.BlitzballPrizes[i]
            );
            if (currentPrize != null)
            {
                prizeButton.Content = currentPrize.Name;
            }
            else
            {
                prizeButton.Content = $"???? [{blitzData.BlitzballPrizes[i]:X2}]";
            }
        }

        this._refresh = false;
    }

    void Prize_OnClick(object sender, RoutedEventArgs e)
    {
        var prizeButton = sender as Button;
        var prizeDialog = new SearchDialog(
            BlitzballValues.Prizes.Select(prize => prize.Name).ToList(),
            string.Empty,
            false
        );
        var dialogResult = prizeDialog.ShowDialog();
        if (dialogResult == false)
        {
            return;
        }

        var prizeNumber = int.Parse(prizeButton.Name.Substring(5));
        var prizeIndex = BlitzballValues.Prizes[prizeDialog.ResultIndex].Index;
        var blitzData = Blitzball.ReadBlitzballData();
        blitzData.BlitzballPrizes[prizeNumber] = (ushort)prizeIndex;
        Blitzball.WriteBlitzballData(blitzData);

        this.Refresh();
    }

    void LeagueTeam_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (this._refresh)
        {
            return;
        }

        var teamBox = sender as ComboBox;
        var teamNum = int.Parse(teamBox.Name.Substring(10));
        var teamIndex = BlitzballValues.Teams[teamBox.SelectedIndex].Index;
        var blitzData = Blitzball.ReadBlitzballData();
        blitzData.LeagueMatchups[teamNum] = (byte)teamIndex;
        Blitzball.WriteBlitzballData(blitzData);
        this.Refresh();
    }

    void CurrentRound_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (this._refresh)
        {
            return;
        }

        var roundBox = sender as ComboBox;
        var roundIndex = BlitzballValues.LeagueStates[roundBox.SelectedIndex].Index;
        var blitzData = Blitzball.ReadBlitzballData();
        blitzData.LeagueStatus = (byte)roundIndex;
        Blitzball.WriteBlitzballData(blitzData);
        this.Refresh();
    }
}
