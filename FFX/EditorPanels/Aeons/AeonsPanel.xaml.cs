using System.Windows.Controls;
using Farplane.FFX.Data;
using Farplane.FFX.Values;
using MahApps.Metro.Controls;

namespace Farplane.FFX.EditorPanels.Aeons;

/// <summary>
/// Interaction logic for AeonsPanel.xaml
/// </summary>
public partial class AeonsPanel : UserControl
{
    public delegate void UpdateTabsDelegate();
    public static event UpdateTabsDelegate UpdateTabsEvent;
    int _currentAeon = 8;
    readonly AeonStats _aeonStats = new();
    readonly AeonAbilities _aeonAbilities = new();

    public AeonsPanel()
    {
        this.InitializeComponent();
        foreach (var item in this.TabAeon.Items)
        {
            ControlsHelper.SetHeaderFontSize((TabItem)item, 14);
        }

        UpdateTabsEvent += this.Refresh;
    }

    public static void UpdateTabs() => UpdateTabsEvent?.Invoke();

    static readonly string[] DefaultAeonNames =
        ["Valefor", "Ifrit", "Ixion", "Shiva", "Bahamut", "Anima", "Yojimbo", "Cindy", "Sandy", "Mindy"];

    public void Refresh()
    {
        for (var i = 0; i < 10; i++)
        {
            var aeonTab = (TabItem)this.TabAeon.Items[i];
            var name = AeonName.GetName(i + 8);
            if (string.IsNullOrEmpty(name) || name.TrimEnd('?').Length == 0)
                name = DefaultAeonNames[i];
            aeonTab.Header = name;
        }
        this._aeonStats.Refresh(this._currentAeon);
        this._aeonAbilities.Refresh(this._currentAeon);
    }

    void TabAeonTab_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var itemIndex = this.TabAeonTab.Items.IndexOf(e.AddedItems[0]);
        this.Refresh();
        switch (itemIndex)
        {
            case 0:
                this.ContentAeon.Content = this._aeonStats;
                break;
            case 1:
                this.ContentAeon.Content = this._aeonAbilities;
                break;
        }
    }

    void TabAeon_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        this._currentAeon = this.TabAeon.Items.IndexOf(e.AddedItems[0]) + 8;
        this.Refresh();
    }
}
