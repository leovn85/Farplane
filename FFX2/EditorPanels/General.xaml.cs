using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Farplane.FFX2.EditorPanels;

/// <summary>
/// Interaction logic for PartyEditor.xaml
/// </summary>
public partial class General : UserControl
{
    readonly int _offsetCurrentGil = (int)OffsetType.CurrentGil;

    public General() => this.InitializeComponent();

    public void Refresh() =>
        this.NumGil.Text = LegacyMemoryReader.ReadUInt32(this._offsetCurrentGil).ToString();

    void NumGil_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        var parsed = int.TryParse(this.NumGil.Text, out var gil);
        if (!parsed)
        {
            // error parsing gil
            MessageBox.Show(
                "The value you entered was invalid.",
                "Error parsing gil",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
            return;
        }

        LegacyMemoryReader.WriteBytes(this._offsetCurrentGil, BitConverter.GetBytes((uint)gil));
    }
}
