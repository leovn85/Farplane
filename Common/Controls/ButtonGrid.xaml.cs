using System.Windows;
using System.Windows.Controls;

namespace Farplane.Common.Controls;

/// <summary>
/// Interaction logic for ButtonGrid.xaml
/// </summary>
public partial class ButtonGrid : UserControl
{
    public delegate void ButtonClickedDelegate(int buttonIndex);
    public event ButtonClickedDelegate ButtonClicked;

    int ButtonCount => this.GridBase.Children.Count;

    public Button[] Buttons
    {
        get
        {
            var buttons = new Button[this.ButtonCount];
            for (var i = 0; i < this.ButtonCount; i++)
            {
                buttons[i] = (Button)this.GridBase.Children[i];
            }
            return buttons;
        }
    }

    public Button this[int buttonIndex] => this.GridBase.Children[buttonIndex] as Button;

    public bool ShowScrollBar
    {
        get => this.ScrollViewer.VerticalScrollBarVisibility == ScrollBarVisibility.Visible;
        set =>
            this.ScrollViewer.VerticalScrollBarVisibility = value
                ? ScrollBarVisibility.Visible
                : ScrollBarVisibility.Hidden;
    }

    public ButtonGrid(int columns, int buttons)
    {
        this.InitializeComponent();

        for (var c = 0; c < columns; c++)
        {
            this.GridBase.ColumnDefinitions.Add(new ColumnDefinition());
        }

        for (var r = 0; r < (buttons / columns); r++)
        {
            this.GridBase.RowDefinitions.Add(new RowDefinition());
        }

        if (buttons % columns == 1)
        {
            this.GridBase.RowDefinitions.Add(new RowDefinition());
        }

        for (var b = 0; b < buttons; b++)
        {
            var column = b % columns;
            var row = b / columns;
            var newButton = new Button()
            {
                Name = "Button" + b,
                Content = "BUTTON",
                Margin = new Thickness(1),
            };
            Grid.SetRow(newButton, row);
            Grid.SetColumn(newButton, column);

            newButton.Click += this.GridButton_Click;
            this.GridBase.Children.Add(newButton);
        }
    }

    void GridButton_Click(object sender, RoutedEventArgs routedEventArgs)
    {
        if (sender is not Button button)
        {
            return;
        }

        var buttonIndex = this.GridBase.Children.IndexOf(button);

        ButtonClicked?.Invoke(buttonIndex);
    }

    public void SetContent(int buttonIndex, object content)
    {
        var button = (Button)this.GridBase.Children[buttonIndex];
        if (button == null)
        {
            return;
        }

        button.Content = content;
        button.UpdateLayout();
    }
}
