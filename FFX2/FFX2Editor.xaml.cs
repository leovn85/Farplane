using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Farplane.FFX2.EditorPanels;
using Farplane.FFX2.EditorPanels.Party;
using Farplane.Memory;
using MahApps.Metro.Controls;
using Image = System.Windows.Controls.Image;

namespace Farplane.FFX2;

/// <summary>
/// Interaction logic for FFX2Editor.xaml
/// </summary>
public partial class FFX2Editor : MetroWindow
{
    readonly General _generalPanel = new();
    readonly PartyPanel _partyPanel = new();
    readonly CreaturePanel _creaturePanel = new();
    readonly CreatureTrapping _trappingPanel = new();
    readonly ItemsEditor _itemsPanel = new();
    readonly DressphereEditor _dresspheresPanel = new();
    readonly AccessoriesEditor _accessoriesPanel = new();
    readonly GarmentGridEditor _garmentGridsPanel = new();
    readonly DebugOptions _debugOptionsPanel = new();
    readonly int _defaultHeight = 540;
    readonly int _defaultWidth = 640;
    bool _rolledUp = false;
    bool _windowPinned = false;
    readonly BitmapImage _iconShrink = new(
        new Uri("pack://application:,,,/Resources/Images/shrink.png")
    );
    readonly BitmapImage _iconExpand = new(
        new Uri("pack://application:,,,/Resources/Images/expand.png")
    );

    public FFX2Editor()
    {
        this.InitializeComponent();
        GameMemory.ProcessExited += this.Close;
    }

    protected override void OnClosed(EventArgs e)
    {
        // Gỡ bỏ sự kiện tĩnh để tránh giữ tham chiếu Window và gây rò rỉ bộ nhớ
        GameMemory.ProcessExited -= this.Close;
        base.OnClosed(e);
    }

    void TreeView_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        var item = (TreeViewItem)e.NewValue;
        if (item == null)
        {
            return;
        }

        switch (item.Name)
        {
            case "GeneralPanel":
                this._generalPanel.Refresh();
                this.EditorPanel.Content = this._generalPanel;
                break;
            case "PartyPanel":
                this._partyPanel.Refresh();
                this.EditorPanel.Content = this._partyPanel;
                break;
            case "CreaturePanel":
                this._creaturePanel.Refresh();
                this.EditorPanel.Content = this._creaturePanel;
                break;
            case "TrappingPanel":
                this._trappingPanel.Refresh();
                this.EditorPanel.Content = this._trappingPanel;
                break;
            case "ItemsPanel":
                this._itemsPanel.Refresh();
                this.EditorPanel.Content = this._itemsPanel;
                break;
            case "AccessoriesPanel":
                this._accessoriesPanel.Refresh();
                this.EditorPanel.Content = this._accessoriesPanel;
                break;
            case "DresspheresPanel":
                this._dresspheresPanel.Refresh();
                this.EditorPanel.Content = this._dresspheresPanel;
                break;
            case "GarmentGridsPanel":
                this._garmentGridsPanel.Refresh();
                this.EditorPanel.Content = this._garmentGridsPanel;
                break;
            case "DebugOptionsPanel":
                this._debugOptionsPanel.Refresh();
                this.EditorPanel.Content = this._debugOptionsPanel;
                break;
            default:
                break;
        }
    }

    void RefreshAll()
    {
        this._generalPanel.Refresh();
        this._partyPanel.Refresh();
        this._creaturePanel.Refresh();
        this._trappingPanel.Refresh();
        this._itemsPanel.Refresh();
        this._accessoriesPanel.Refresh();
        this._dresspheresPanel.Refresh();
        this._debugOptionsPanel.Refresh();
        this._garmentGridsPanel.Refresh();
    }

    void RefreshAll_Click(object sender, RoutedEventArgs e) => this.RefreshAll();

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
            this.Left -= this._defaultWidth - 210;
            this.GridContent.Visibility = Visibility.Visible;
            this.Width = this._defaultWidth;
            this.Height = this._defaultHeight;
            this.ButtonRollUp.Content = new Image
            {
                Source = this._iconShrink,
                Width = 16,
                Height = 16,
            };
        }
        else
        {
            this.Width = 210;
            this.Left += this._defaultWidth - this.Width;
            this.Height = 30;
            this.GridContent.Visibility = Visibility.Hidden;
            this.ButtonRollUp.Content = new Image
            {
                Source = this._iconExpand,
                Width = 16,
                Height = 16,
            };
        }
        this._rolledUp = !this._rolledUp;
    }
}
