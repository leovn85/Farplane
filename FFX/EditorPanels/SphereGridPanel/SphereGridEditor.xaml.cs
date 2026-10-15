using System.Windows;
using System.Windows.Controls;
using Farplane.Common;
using Farplane.FFX.Data;

namespace Farplane.FFX.EditorPanels.SphereGridPanel;

/// <summary>
///     Interaction logic for SphereGridEditor.xaml
/// </summary>
public partial class SphereGridEditor : UserControl
{
    bool _refreshing;
    int _currentNode;

    public SphereGridEditor()
    {
        this.InitializeComponent();
        this.ComboNodeType.ItemsSource = SphereGrid.GetNames();
    }

    public void Refresh()
    {
        this._refreshing = true;
        this.RefreshNode();
        this._refreshing = false;
    }

    void RefreshNode()
    {
        var node = SphereGrid.ReadNode(this._currentNode);

        this.TextCurrentNode.Text = $"Currently editing node #{this._currentNode}";
        this.ComboNodeType.SelectedIndex = node.NodeType;

        var activations = BitHelper.GetBitArray([node.ActivatedBy], 8);
        for (var i = 0; i < 7; i++)
        {
            if (this.PanelNodeActivatedBy.Children[i] is CheckBox checkBox)
            {
                checkBox.IsChecked = activations[i];
            }
        }
    }

    void ComboNodeType_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (this._refreshing)
        {
            return;
        }

        SphereGrid.SetNodeType(
            this._currentNode,
            SphereGrid.NodeTypes[this.ComboNodeType.SelectedIndex].ID
        );
    }

    void SphereGridActivation_Changed(object sender, RoutedEventArgs e)
    {
        if (this._refreshing)
        {
            return;
        }

        var senderBox = sender as CheckBox;
        var senderIndex = this.PanelNodeActivatedBy.Children.IndexOf(senderBox);

        var current = SphereGrid.ReadNode(this._currentNode);
        var actCurrent = current.ActivatedBy;
        actCurrent = BitHelper.ToggleBit(actCurrent, senderIndex);
        SphereGrid.SetNodeActivation(this._currentNode, actCurrent);
        this.Refresh();
    }

    void ButtonSelectNode_Click(object sender, RoutedEventArgs e)
    {
        var selectedNode = SphereGrid.GetSelectedNode();
        this._currentNode = selectedNode;

        this.Refresh();
    }
}
