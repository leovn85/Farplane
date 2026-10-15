using System.Windows.Controls;
using System.Windows.Input;
using Farplane.Common;
using Farplane.Memory;
using MahApps.Metro.Controls;

namespace Farplane.FFX2.EditorPanels;

/// <summary>
/// Interaction logic for CreatureEditor.xaml
/// </summary>
public partial class CreatureEditor : UserControl
{
    readonly int _creatureIndex = -1;
    readonly int _statsOffset = 0;
    readonly CreatureAbilities _creatureAbilities;
    readonly StatsPanel _statsPanel;
    bool _refreshing = false;

    readonly int _offsetCreatureBase = (int)OffsetType.PartyStatBase;
    readonly int _offsetCreatureName = (int)OffsetType.CreatureNames;

    public CreatureEditor(int creatureIndex)
    {
        this._creatureIndex = creatureIndex;
        this._statsOffset = this._offsetCreatureBase + ((creatureIndex + 15) * 0x80);
        this.InitializeComponent();

        this._creatureAbilities = new CreatureAbilities(creatureIndex);
        this._statsPanel = new StatsPanel();

        this.ccStats.Content = this._statsPanel;
        this.TabAbilities.Content = this._creatureAbilities;
        this.Refresh();

        foreach (TabItem tabControl in this.CreatureTab.Items)
        {
            ControlsHelper.SetHeaderFontSize(tabControl, 18);
        }
    }

    public void Refresh()
    {
        this._refreshing = true;
        var nameBytes = LegacyMemoryReader.ReadBytes(
            this._offsetCreatureName + (this._creatureIndex * 40),
            18
        );
        var name = StringConverter.ToASCII(nameBytes);
        this.CreatureName.Text = name;
        this.CreatureSize.SelectedIndex =
            GameMemory.Read<byte>(this._statsOffset + (int)Offsets.StatOffsets.Size) - 1;
        this._statsPanel.Refresh(this._creatureIndex + 15);
        this._creatureAbilities.Refresh();
        this._refreshing = false;
    }

    void CreatureName_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        var nameBytes = StringConverter.ToFFX(this.CreatureName.Text);
        var writeBytes = new byte[18];
        nameBytes.CopyTo(writeBytes, 0);
        LegacyMemoryReader.WriteBytes(
            this._offsetCreatureName + (this._creatureIndex * 40),
            writeBytes
        );
        this.Refresh();
    }

    void CreatureSize_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (this._refreshing)
        {
            return;
        }

        LegacyMemoryReader.WriteBytes(
            this._statsOffset + (int)Offsets.StatOffsets.Size,
            [(byte)(this.CreatureSize.SelectedIndex + 1)]
        );
    }
}
