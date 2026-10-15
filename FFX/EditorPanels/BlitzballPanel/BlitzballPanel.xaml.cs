using System.Windows.Controls;

namespace Farplane.FFX.EditorPanels.BlitzballPanel;

/// <summary>
/// Interaction logic for BlitzballPanel.xaml
/// </summary>
public partial class BlitzballPanel : UserControl
{
    public BlitzballPanel() => this.InitializeComponent();

    public void Refresh()
    {
        this.LeagueEditor.Refresh();
        this.TournamentEditor.Refresh();
        this.PlayerEditor.Refresh();
        this.TeamEditor.Refresh();
    }
}
