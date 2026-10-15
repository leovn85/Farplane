using System.Windows.Controls;

namespace Farplane.FFX2.EditorPanels.Party;

/// <summary>
/// Interaction logic for PartyPanel.xaml
/// </summary>
public partial class PartyPanel : UserControl
{
    int _selected = 0;
    bool _refresh;
    readonly StatsPanel _statsPanel = new();
    readonly DressphereAbilities _dressphereAbilities = new();

    public PartyPanel()
    {
        this._refresh = true;

        this.InitializeComponent();

        this._refresh = false;
    }

    public void Refresh()
    {
        this._refresh = true;

        this._statsPanel.Refresh(this._selected);

        this._dressphereAbilities.SelectedIndex = this._selected;
        this._dressphereAbilities.RefreshAbilities();
        this._dressphereAbilities.ReloadDresspheres();

        this._refresh = false;
    }

    void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (this._refresh)
        {
            return;
        }

        this._selected = this.TabParty.SelectedIndex;
        this.Refresh();
    }

    void TabEditor_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (this.TabEditor.SelectedIndex == 0)
        {
            this.PartyEditor.Content = this._statsPanel;
        }
        else
        {
            this.PartyEditor.Content = this._dressphereAbilities;
        }
    }
}
