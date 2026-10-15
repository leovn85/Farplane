using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Farplane.FFX2.Values;

namespace Farplane.FFX2.EditorPanels;

/// <summary>
/// Interaction logic for ItemsEditor.xaml
/// </summary>
public partial class AccessoriesEditor : UserControl
{
    byte[] itemTypes;
    byte[] itemCounts;
    Button editingButton = null;
    int editingItem = -1;

    public AccessoriesEditor()
    {
        this.InitializeComponent();
        this.ItemsGrid.ColumnDefinitions.Add(new ColumnDefinition());
        this.ItemsGrid.ColumnDefinitions.Add(new ColumnDefinition());
        for (var r = 0; r < 64; r++)
        {
            this.ItemsGrid.RowDefinitions.Add(new RowDefinition());
        }
        for (var i = 0; i < 128; i++)
        {
            var row = i / 2;
            var col = i % 2 == 0 ? 0 : 1;
            var itemButton = new Button() { Name = "Accessory" + i };
            itemButton.Click += (sender, args) =>
            {
                if (sender is not Button btn)
                    return;
                var itemNum = int.Parse(btn.Name.Substring(9));
                if (itemNum == this.editingItem && this.editingButton != null)
                {
                    return;
                }
                this.SetButtonBox(btn, itemNum);
                if (btn.Content is StackPanel panel && panel.Children.Count > 1)
                {
                    panel.UpdateLayout();
                    panel.Children[1].Focus();
                }
            };
            this.ItemsGrid.Children.Add(itemButton);
            Grid.SetRow(itemButton, row);
            Grid.SetColumn(itemButton, col);
        }
    }
    public void Refresh()
    {
        this.itemTypes = LegacyMemoryReader.ReadBytes((int)OffsetType.AccessoryType, 0x100);
        this.itemCounts = LegacyMemoryReader.ReadBytes((int)OffsetType.AccessoryCount, 0x80);
        for (var i = 0; i < 128; i++)
        {
            var button = (Button)this.ItemsGrid.Children[i];
            this.SetButtonText(button, i);
        }
    }

    void ResetButton()
    {
        if (this.editingButton != null)
        {
            this.SetButtonText(this.editingButton, this.editingItem);
            this.editingButton = null;
            this.editingItem = -1;
        }
    }

    public void SetButtonText(Button button, int itemNum)
    {
        var itemType = this.itemTypes[itemNum * 2];
        var itemCount = this.itemCounts[itemNum];

        var itemNames = Accessories.GetNames();

        if (itemType >= 128 || itemCount == 0)
        {
            button.Content = "EMPTY";
        }
        else
        {
            button.Content = $"{itemNames[itemType]}  x{itemCount}";
        }
    }

    public void SetButtonBox(Button button, int itemNum)
    {
        this.ResetButton();

        var textItemCount = new TextBox { Text = this.itemCounts[itemNum].ToString() };
        textItemCount.SelectionStart = textItemCount.Text.Length;

        var comboItemType = new ComboBox { Padding = new Thickness(0D) };
        var accessories = Accessories.GetNames();
        accessories.Insert(0, "None");
        comboItemType.ItemsSource = accessories;
        var selItem = this.itemTypes[itemNum * 2];
        comboItemType.SelectedIndex = selItem > 127 ? 0 : selItem + 1;
        comboItemType.Width = 140;

        var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal };

        buttonPanel.Children.Add(comboItemType);
        buttonPanel.Children.Add(textItemCount);

        buttonPanel.KeyDown += (o, eventArgs) =>
        {
            switch (eventArgs.Key)
            {
                case Key.Enter:
                    var newCount = byte.Parse(textItemCount.Text);
                    var newItem = comboItemType.SelectedIndex - 1;
                    if (newItem != -1 && newCount > 0)
                    {
                        Cheats.WriteAccessory(itemNum, newItem, newCount);
                    }
                    else
                    {
                        Cheats.WriteAccessory(itemNum, -1, 0);
                    }

                    this.ResetButton();
                    this.Refresh();
                    break;
                case Key.Escape:
                    this.ResetButton();
                    break;
                default:
                    break;
            }
        };

        button.Content = buttonPanel;

        this.editingButton = button;
        this.editingItem = itemNum;
    }

    void GiveAllItems_Click(object sender, RoutedEventArgs e)
    {
        Cheats.GiveAllAccessories();
        this.Refresh();
    }
}
