using System.Windows.Controls;
using Farplane.FFX.Data;

namespace Farplane.FFX.EditorPanels.SkillEditorPanel;

/// <summary>
/// Interaction logic for CommandEditor.xaml
/// </summary>
public partial class CommandEditor : UserControl
{
    public CommandEditor() => this.InitializeComponent();

    public void Refresh()
    {
        this.ListCommands.Items.Clear();
        var skillNames = Skills.GetSkillNames();
        foreach (var skill in skillNames)
        {
            this.ListCommands.Items.Add(skill);
        }
    }

    void ListCommands_OnSelectionChanged(object sender, SelectionChangedEventArgs e) =>
        this.Description.Text = Skills.GetSkillDescription(this.ListCommands.SelectedIndex);
}
