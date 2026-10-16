using System.Windows.Controls;

namespace Farplane.FFX.EditorPanels.SkillEditorPanel;

public partial class SkillEditorPanel : UserControl
{
    public SkillEditorPanel() => this.InitializeComponent();

    public void Refresh() => this.CommandEditor.Refresh();
}
