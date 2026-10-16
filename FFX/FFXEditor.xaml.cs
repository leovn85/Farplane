using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Farplane.FFX.EditorPanels;
using Farplane.FFX.EditorPanels.Aeons;
using Farplane.FFX.EditorPanels.Battle;
using Farplane.FFX.EditorPanels.BlitzballPanel;
using Farplane.FFX.EditorPanels.Boosters;
using Farplane.FFX.EditorPanels.Debug;
using Farplane.FFX.EditorPanels.EquipmentPanel;
using Farplane.FFX.EditorPanels.GeneralPanel;
using Farplane.FFX.EditorPanels.ItemsPanel;
using Farplane.FFX.EditorPanels.MonsterArenaPanel;
using Farplane.FFX.EditorPanels.PartyPanel;
using Farplane.FFX.EditorPanels.SkillEditorPanel;
using Farplane.FFX.EditorPanels.SphereGridPanel;
using Farplane.Memory;
using MahApps.Metro.Controls;
using MessageBox = System.Windows.MessageBox;
using TreeView = System.Windows.Controls.TreeView;

namespace Farplane.FFX;

/// <summary>
/// Interaction logic for FFXEditor.xaml
/// </summary>
public partial class FFXEditor : MetroWindow
{
    readonly GeneralPanel _generalPanel;
    readonly PartyPanel _partyPanel;
    readonly AeonsPanel _aeonsPanel;
    readonly ItemsPanel _itemsPanel;
    readonly SphereGridPanel _sphereGridPanel;
    readonly EquipmentPanel _equipmentPanel;
    readonly BlitzballPanel _blitzballPanel;
    readonly MonsterArenaPanel _monsterArenaPanel;
    readonly SkillEditorPanel _skillEditorPanel;
    readonly DebugPanel _debugPanel;
    readonly BattlePanel _battlePanel;
    readonly BoostersPanel _boostersPanel;
    readonly NotAvailablePanel _notAvailablePanel = new();
    bool _rolledUp = false;
    bool _windowPinned = false;
    readonly BitmapImage _iconShrink = new(
        new Uri("pack://application:,,,/Resources/Images/shrink.png")
    );
    readonly BitmapImage _iconExpand = new(
        new Uri("pack://application:,,,/Resources/Images/expand.png")
    );

    public FFXEditor(bool fileMode = false)
    {
        this.InitializeComponent();
        if (fileMode)
        {
            // Set up window for file mode
        }
        else
        {
            // Set up window for process mode
            GameMemory.ProcessExited += this.Close;
            this._generalPanel = new GeneralPanel();
            this._partyPanel = new PartyPanel();
            this._aeonsPanel = new AeonsPanel();
            this._itemsPanel = new ItemsPanel();
            this._sphereGridPanel = new SphereGridPanel();
            this._equipmentPanel = new EquipmentPanel();
            this._blitzballPanel = new BlitzballPanel();
            this._monsterArenaPanel = new MonsterArenaPanel();
            this._debugPanel = new DebugPanel();
            this._battlePanel = new BattlePanel();
            this._boostersPanel = new BoostersPanel();
        }
        // Set up general window parameters
        this._skillEditorPanel = new SkillEditorPanel();
        (this.EditorTree.Items[0] as TreeViewItem).IsSelected = true;
    }

    protected override void OnClosed(EventArgs e)
    {
        this._boostersPanel?.Stop();
        // Unsubscribe static event to prevent memory leaks
        GameMemory.ProcessExited -= this.Close;
        base.OnClosed(e);
    }

    void EditorTree_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        var treeView = sender as TreeView;
        if (treeView?.SelectedItem is not TreeViewItem treeViewItem)
        {
            return;
        }
        try
        {
            switch (treeViewItem.Name)
            {
                case "GeneralEditor":
                    this._generalPanel.Refresh();
                    this.EditorContent.Content = this._generalPanel;
                    break;
                case "PartyEditor":
                    this._partyPanel.Refresh();
                    this.EditorContent.Content = this._partyPanel;
                    break;
                case "AeonEditor":
                    this._aeonsPanel.Refresh();
                    this.EditorContent.Content = this._aeonsPanel;
                    break;
                case "ItemEditor":
                    this._itemsPanel.Refresh();
                    this.EditorContent.Content = this._itemsPanel;
                    break;
                case "SphereGridEditor":
                    this._sphereGridPanel.Refresh();
                    this.EditorContent.Content = this._sphereGridPanel;
                    break;
                case "EquipmentEditor":
                    this._equipmentPanel.Refresh();
                    this.EditorContent.Content = this._equipmentPanel;
                    break;
                case "BlitzballEditor":
                    this._blitzballPanel.Refresh();
                    this.EditorContent.Content = this._blitzballPanel;
                    break;
                case "MonsterArenaEditor":
                    this._monsterArenaPanel.Refresh();
                    this.EditorContent.Content = this._monsterArenaPanel;
                    break;
                case "BattleEditor":
                    this._battlePanel.Refresh();
                    this.EditorContent.Content = this._battlePanel;
                    break;
                case "SkillEditor":
                    this._skillEditorPanel.Refresh();
                    this.EditorContent.Content = this._skillEditorPanel;
                    break;
                case "DebugEditor":
                    this._debugPanel.Refresh();
                    this.EditorContent.Content = this._debugPanel;
                    break;
                case "Boosters":
                    this._boostersPanel.Refresh();
                    this.EditorContent.Content = this._boostersPanel;
                    break;
                default:
                    this.EditorContent.Content = this._notAvailablePanel;
                    break;
            }
        }
        catch (NullReferenceException)
        {
            this.EditorContent.Content = this._notAvailablePanel;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Exception loading panel:\n{ex.Message}");
        }
    }

    public void RefreshAllPanels()
    {
        this._generalPanel?.Refresh();
        this._partyPanel?.Refresh();
        this._aeonsPanel?.Refresh();
        this._itemsPanel?.Refresh();
        this._sphereGridPanel?.Refresh();
        this._equipmentPanel?.Refresh();
        this._blitzballPanel?.Refresh();
        this._monsterArenaPanel?.Refresh();
        this._battlePanel?.Refresh();
        this._debugPanel?.Refresh();
        this._boostersPanel?.Refresh();
        this._skillEditorPanel?.Refresh();
    }

    void RefreshAll_Click(object sender, RoutedEventArgs e) => this.RefreshAllPanels();

    void ButtonPin_Click(object sender, RoutedEventArgs e)
    {
        this._windowPinned = !this._windowPinned;
        this.ButtonPin.IsChecked = this._windowPinned;
        this.Topmost = this._windowPinned;
    }

    void ButtonRollUp_Click(object sender, RoutedEventArgs e)
    {
        if (this._rolledUp)
        {
            var expandWindow = (Storyboard)this.Resources["ExpandWindow"];
            this.BeginStoryboard(expandWindow, HandoffBehavior.SnapshotAndReplace);
        }
        else
        {
            var shrinkWindow = (Storyboard)this.Resources["ShrinkWindow"];
            this.BeginStoryboard(shrinkWindow, HandoffBehavior.SnapshotAndReplace);
        }
        this._rolledUp = !this._rolledUp;
    }
}
