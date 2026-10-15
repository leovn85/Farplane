using System.Windows.Controls;

namespace Farplane.FFX.EditorPanels.SphereGridPanel;

/// <summary>
/// Interaction logic for SphereGridPanel.xaml
/// </summary>
public partial class SphereGridPanel : UserControl
{
    readonly SphereGridEditor _sphereGridEditor = new();

    public SphereGridPanel()
    {
        this.InitializeComponent();
        this.SphereGridEditor.Content = this._sphereGridEditor;
    }

    public void Refresh() => this._sphereGridEditor.Refresh();
}
