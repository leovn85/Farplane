using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Farplane.FFX2.Values;

namespace Farplane.FFX2.EditorPanels;

/// <summary>
/// Interaction logic for ItemsEditor.xaml
/// </summary>
public partial class ItemsEditor : UserControl
{
    readonly int _offsetItemType = (int)OffsetType.ItemType;
    readonly int _offsetItemCount = (int)OffsetType.ItemCount;
    byte[] itemTypes;
    byte[] itemCounts;
    Button editingButton = null;
    int editingItem = -1;

    public ItemsEditor()
    {
        this.InitializeComponent();
        this.ItemsGrid.ColumnDefinitions.Add(new ColumnDefinition());
        this.ItemsGrid.ColumnDefinitions.Add(new ColumnDefinition());
        for (var r = 0; r < 34; r++)
        {
            this.ItemsGrid.RowDefinitions.Add(new RowDefinition());
        }
        for (var i = 0; i < 68; i++)
        {
            var row = i / 2;
            var col = i % 2 == 0 ? 0 : 1;
            var itemButton = new Button() { Name = "Item" + i };
            itemButton.Click += (sender, args) =>
            {
                if (sender is not Button btn)
                    return;
                var itemNum = int.Parse(btn.Name.Substring(4));
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
        this.itemTypes = LegacyMemoryReader.ReadBytes(this._offsetItemType, 0x88);
        this.itemCounts = LegacyMemoryReader.ReadBytes(this._offsetItemCount, 0x44);
        for (var i = 0; i < 68; i++)
        {
            var button = (Button)this.ItemsGrid.Children[i];
            if (button == null)
            {
                continue;
            }
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

        if (itemType >= 68 || itemCount == 0)
        {
            button.Content = "EMPTY";
        }
        else
        {
            button.Content = $"{Items.ItemNames[itemType + 1]}  x{itemCount}";
        }
    }

    public void SetButtonBox(Button button, int itemNum)
    {
        this.ResetButton();

        var textItemCount = new TextBox { Text = this.itemCounts[itemNum].ToString() };
        textItemCount.SelectionStart = textItemCount.Text.Length;

        var comboItemType = new ComboBox
        {
            Padding = new Thickness(0D),
            ItemsSource = Items.ItemNames,
        };
        var selItem = this.itemTypes[itemNum * 2];
        comboItemType.SelectedIndex = selItem > 68 ? 0 : selItem + 1;
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
                        Cheats.WriteItem(itemNum, newItem, newCount);
                    }
                    else
                    {
                        Cheats.WriteItem(itemNum, -1, 0);
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
        Cheats.GiveAllItems();
        this.Refresh();
    }
}

public class InventoryItem
{
    public int ItemSlot { get; set; }
    public int ItemID { get; set; }
    public int ItemCount { get; set; }
}
