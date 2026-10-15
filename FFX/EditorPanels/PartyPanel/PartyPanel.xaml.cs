using System.Windows.Controls;
using Farplane.FFX.Values;
using MahApps.Metro.Controls;

namespace Farplane.FFX.EditorPanels.PartyPanel;

/// <summary>
/// Interaction logic for PartyPanel.xaml
/// </summary>
public partial class PartyPanel : UserControl
{
    bool _refreshing = false;
    int _selectedIndex = -1;

    public PartyPanel()
    {
        this.InitializeComponent();
        foreach (var tab in this.TabPartySelect.Items)
        {
            ControlsHelper.SetHeaderFontSize((TabItem)tab, 14);
        }

        this.Refresh();
    }

    public void Refresh()
    {
        this._refreshing = true;

        if (this._selectedIndex == -1)
        {
            this.TabPartySelect.SelectedIndex = 0;
            this._selectedIndex = 0;
        }

        this.PartyEditor.Load((Character)this._selectedIndex);
        this.PartyEditor.Refresh();

        this._refreshing = false;
    }

    void TabPartySelect_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (this._refreshing)
        {
            return;
        }

        this._selectedIndex = this.TabPartySelect.SelectedIndex;
        this.PartyEditor.Load((Character)this._selectedIndex);
        this.PartyEditor.Refresh();
    }
}
