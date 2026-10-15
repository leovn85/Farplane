using System.Windows.Controls;

namespace Farplane.FFX.EditorPanels;

/// <summary>
/// Interaction logic for NotImplementedPanel.xaml
/// </summary>
public partial class NotAvailablePanel : UserControl
{
    public NotAvailablePanel() => this.InitializeComponent();

    public void SetText(string header, string text)
    {
        this.TextNYI.Text = text;
        this.GroupNYI.Header = header;
    }
}
