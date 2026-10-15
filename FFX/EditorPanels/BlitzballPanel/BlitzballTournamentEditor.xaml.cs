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
public partial class BlitzballTournamentEditor : UserControl
{
    bool _refresh;

    public BlitzballTournamentEditor()
    {
        this.InitializeComponent();

        this._refresh = true;
        this.ComboTournamentStatus.ItemsSource = BlitzballValues.TournamentStates.Select(state =>
            state.Name
        );
        foreach (
            var teamCombo in this.FindChildren<ComboBox>()
                .Where(child =>
                    child.Name.StartsWith("ComboWinner") || child.Name.StartsWith("ComboTeam")
                )
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

        // Tournament status
        var tournamentStatusIndex = Array.IndexOf(
            BlitzballValues.TournamentStates,
            BlitzballValues.TournamentStates.First(state =>
                state.Index == blitzData.TournamentStatus
            )
        );

        this.ComboTournamentStatus.SelectedIndex = tournamentStatusIndex;

        // Prizes
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

        // Tournament teams
        for (var i = 0; i < 6; i++)
        {
            if (this.FindName($"ComboTeam{i}") is not ComboBox comboTeam)
            {
                continue;
            }

            var currentTeam = BlitzballValues.Teams.First(team =>
                team.Index == blitzData.TournamentMatchups[i]
            );
            comboTeam.SelectedIndex = Array.IndexOf(BlitzballValues.Teams, currentTeam);
        }

        // Tournament winners
        for (var i = 0; i < 8; i++)
        {
            if (this.FindName($"ComboWinner{i}") is not ComboBox comboTeam)
            {
                continue;
            }

            var currentTeam = BlitzballValues.Teams.First(team =>
                team.Index == blitzData.TournamentWinners[i]
            );
            comboTeam.SelectedIndex = Array.IndexOf(BlitzballValues.Teams, currentTeam);
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

    void ComboTeam_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (this._refresh)
        {
            return;
        }

        var teamBox = sender as ComboBox;
        var teamNum = int.Parse(teamBox.Name.Substring(9));
        var teamIndex = BlitzballValues.Teams[teamBox.SelectedIndex].Index;
        var blitzData = Blitzball.ReadBlitzballData();
        blitzData.TournamentMatchups[teamNum] = (byte)teamIndex;
        Blitzball.WriteBlitzballData(blitzData);
        this.Refresh();
    }

    void ComboWinner_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (this._refresh)
        {
            return;
        }

        var teamBox = sender as ComboBox;
        var teamNum = int.Parse(teamBox.Name.Substring(11));
        var teamIndex = BlitzballValues.Teams[teamBox.SelectedIndex].Index;
        var blitzData = Blitzball.ReadBlitzballData();
        blitzData.TournamentWinners[teamNum] = (byte)teamIndex;
        Blitzball.WriteBlitzballData(blitzData);
        this.Refresh();
    }
}
