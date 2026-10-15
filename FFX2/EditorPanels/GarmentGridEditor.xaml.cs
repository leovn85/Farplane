using System.Windows;
using System.Windows.Controls;
using Farplane.FFX2.Values;

namespace Farplane.FFX2.EditorPanels;

/// <summary>
/// Interaction logic for GarmentGridEditor.xaml
/// </summary>
public partial class GarmentGridEditor : UserControl
{
    readonly int _offsetGarmentGrids = (int)OffsetType.KnownGarmentGrids;
    bool _refreshing;

    public GarmentGridEditor()
    {
        this.InitializeComponent();
        for (var i = 0; i < GarmentGrids.GarmentGridList.Length; i++)
        {
            var gg = GarmentGrids.GarmentGridList[i];

            var ggCheckBox = new CheckBox()
            {
                Name = "Grid" + gg.ID,
                Content = gg.Name,
                Margin = new Thickness(2),
            };
            ggCheckBox.Checked += this.GarmentGridChanged;
            ggCheckBox.Unchecked += this.GarmentGridChanged;

            if (i < GarmentGrids.GarmentGridList.Length / 2)
            {
                this.GarmentGridList.Children.Add(ggCheckBox);
            }
            else
            {
                this.GarmentGridList2.Children.Add(ggCheckBox);
            }
        }
        this.Refresh();
    }

    void GarmentGridChanged(object sender, RoutedEventArgs routedEventArgs)
    {
        if (this._refreshing)
        {
            return;
        }

        if (sender is not CheckBox checkBox)
        {
            return;
        }

        var gridIndex = int.Parse(checkBox.Name.Substring(4));

        var byteIndex = gridIndex / 8;
        var bitIndex = gridIndex % 8;

        var garmentGridBytes = LegacyMemoryReader.ReadBytes(this._offsetGarmentGrids, 8);
        var gByte = garmentGridBytes[byteIndex];

        var mask = 1 << bitIndex;
        var newByte = gByte ^ (byte)mask;

        LegacyMemoryReader.WriteBytes(this._offsetGarmentGrids + byteIndex, [(byte)newByte]);
        this.Refresh();
    }

    public void Refresh()
    {
        this._refreshing = true;
        var garmentGridBytes = LegacyMemoryReader.ReadBytes(this._offsetGarmentGrids, 8);
        var ggLen = GarmentGrids.GarmentGridList.Length;
        for (var i = 0; i < GarmentGrids.GarmentGridList.Length; i++)
        {
            CheckBox checkBox = null;
            if (i < ggLen / 2)
            {
                checkBox = (CheckBox)this.GarmentGridList.Children[i];
            }
            else
            {
                checkBox = (CheckBox)this.GarmentGridList2.Children[i - (ggLen / 2)];
            }

            if (checkBox == null)
            {
                continue;
            }

            var byteIndex = i / 8;
            var bitIndex = i % 8;

            var mask = (byte)(1 << bitIndex);
            var isSet = (garmentGridBytes[byteIndex] & mask) != 0;

            checkBox.IsChecked = isSet;
        }
        this._refreshing = false;
    }

    void GiveAllGrids_Click(object sender, RoutedEventArgs e)
    {
        Cheats.GiveAllGrids();
        this.Refresh();
    }
}
