using System.Windows.Controls;
using Farplane.FFX.Values;

namespace Farplane.FFX.EditorPanels.PartyPanel;

/// <summary>
/// Interaction logic for PartyEditor.xaml
/// </summary>
public partial class PartyEditor : UserControl
{
    int _character = -1;

    public PartyEditor() => this.InitializeComponent();

    public void Refresh()
    {
        if (this._character == -1)
        {
            return;
        }

        this.PartyStats.Refresh(this._character);
        this.PartyAbilities.Refresh(this._character);
        this.PartyOverdrive.Refresh(this._character);
    }

    public void Load(Character character)
    {
        this._character = (int)character;

        this.Refresh();
    }
}
