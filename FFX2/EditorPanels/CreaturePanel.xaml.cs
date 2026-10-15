using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Farplane.Common;
using MahApps.Metro.Controls;

namespace Farplane.FFX2.EditorPanels;

/// <summary>
/// Interaction logic for CreaturePanel.xaml
/// </summary>
public partial class CreaturePanel : UserControl
{
    readonly CreatureEditor[] _editors = new CreatureEditor[8];
    readonly int _offsetCreatureName = (int)OffsetType.CreatureNames;

    public delegate void UpdateCreaturesEvent();
    public static event UpdateCreaturesEvent UpdateCreatures;

    public CreaturePanel()
    {
        this.InitializeComponent();
        for (var i = 0; i < 8; i++)
        {
            this._editors[i] = new CreatureEditor(i);
            var tabCreature = new MetroTabItem
            {
                Name = "Creature" + i,
                Header = "Creature " + i,
                Content = this._editors[i],
            };
            ControlsHelper.SetHeaderFontSize(tabCreature, 12);
            this.TabCreatures.Items.Add(tabCreature);
        }
        this.UpdateCreaturesMethod();
        UpdateCreatures += this.UpdateCreaturesMethod;
    }

    public void Refresh() => UpdateCreatures?.Invoke();

    public void UpdateCreaturesMethod()
    {
        var tabs = this.TabCreatures.Items.SourceCollection.OfType<TabItem>().ToArray();
        var creatureCount = LegacyMemoryReader.ReadByte(0x9FA6C1);

        for (var i = 0; i < 8; i++)
        {
            var creatureTab = tabs[i];
            if (creatureTab == null)
            {
                continue;
            }

            if (i >= creatureCount)
            {
                creatureTab.Visibility = Visibility.Collapsed;
                continue;
            }

            creatureTab.Visibility = Visibility.Visible;

            var nameBytes = LegacyMemoryReader.ReadBytes(this._offsetCreatureName + (i * 40), 18);
            var name = StringConverter.ToASCII(nameBytes);
            creatureTab.Header = name;
            this._editors[i].Refresh();
        }
    }
}
