using System.Windows;
using MahApps.Metro.Controls;

namespace Farplane.FFX2;

/// <summary>
/// Interaction logic for AbilitySlider.xaml
/// </summary>
public partial class AbilitySlider : MetroWindow
{
    public int AP => (int)this.SliderAP.Value;

    public AbilitySlider(int maxValue, int currentValue)
    {
        this.InitializeComponent();

        this.SliderAP.Maximum = currentValue;
        this.SliderAP.Value = currentValue;
        this.SliderAP.Maximum = maxValue;
        this.LabelAP.Content = $"{(int)this.SliderAP.Value} / {(int)this.SliderAP.Maximum}";
    }

    void SliderAP_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) =>
        this.LabelAP.Content = $"{(int)this.SliderAP.Value} / {(int)this.SliderAP.Maximum}";

    void ButtonMaster_OnClick(object sender, RoutedEventArgs e) =>
        this.SliderAP.Value = this.SliderAP.Maximum;

    void ButtonSave_OnClick(object sender, RoutedEventArgs e)
    {
        this.DialogResult = true;
        this.Close();
    }
}
