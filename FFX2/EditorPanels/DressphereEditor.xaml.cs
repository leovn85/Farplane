using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Farplane.FFX2.EditorPanels;

/// <summary>
/// Interaction logic for DressphereEditor.xaml
/// </summary>
public partial class DressphereEditor : UserControl
{
    readonly int _offsetDresspheres = (int)OffsetType.DressphereCountBase;

    public DressphereEditor() => this.InitializeComponent();

    public void Refresh()
    {
        var dressBytes = LegacyMemoryReader.ReadBytes(this._offsetDresspheres, 30);
        for (var d = 1; d <= 29; d++)
        {
            var dressBox = (TextBox)this.FindName("Dressphere" + d);
            if (dressBox == null)
            {
                continue;
            }

            dressBox.Text = dressBytes[d].ToString();
        }
    }

    void DressBox_KeyDown(object sender, KeyEventArgs e)
    {
        var dressBox = (TextBox)sender;
        if (dressBox == null)
        {
            return;
        }

        switch (e.Key)
        {
            case Key.Enter:
                var dressIndex = int.Parse(dressBox.Name.Substring(10));
                var quantity = 0;
                var parsed = int.TryParse(dressBox.Text, out quantity);
                if (parsed && quantity <= 127 && quantity >= 0)
                {
                    LegacyMemoryReader.WriteBytes(
                        this._offsetDresspheres + dressIndex,
                        [(byte)quantity]
                    );
                    this.Refresh();
                }
                else
                {
                    MessageBox.Show(
                        "Please enter a number between 0 and 127.",
                        "Value input error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
                break;
            case Key.Escape:
                this.Refresh();
                dressBox.SelectionStart = 0;
                dressBox.SelectionLength = dressBox.Text.Length;
                break;
            default:
                break;
        }
    }
}
