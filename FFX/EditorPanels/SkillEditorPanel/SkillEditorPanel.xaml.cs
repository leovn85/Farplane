using System.Windows.Controls;

namespace Farplane.FFX.EditorPanels.SkillEditorPanel;

/// <summary>
/// Interaction logic for SkillEditorPanel.xaml
/// </summary>
public partial class SkillEditorPanel : UserControl
{
    public SkillEditorPanel() => this.InitializeComponent();

    public void Refresh() => this.CommandEditor.Refresh();
}
