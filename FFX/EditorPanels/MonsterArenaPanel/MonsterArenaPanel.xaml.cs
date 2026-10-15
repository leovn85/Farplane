using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Farplane.Common;
using Farplane.FFX.Data;
using Farplane.FFX.Values;

namespace Farplane.FFX.EditorPanels.MonsterArenaPanel;

/// <summary>
/// Interaction logic for MonsterArenaPanel.xaml
/// </summary>
public partial class MonsterArenaPanel : UserControl
{
    public MonsterArenaPanel()
    {
        this.InitializeComponent();

        // Initialize area list
        foreach (var area in MonsterArenaData.MonsterArenaAreas)
        {
            this.ListMonsterArenaAreas.Items.Add(new TreeViewItem { Header = area.Name });
        }

        (this.ListMonsterArenaAreas.Items[0] as TreeViewItem).IsSelected = true;
    }

    public void Refresh()
    {
        this.GridMonsterArenaMonsters.Children.Clear();

        var arenaBytes = MonsterArena.GetCaptureCounts();

        var selectedArea = this.ListMonsterArenaAreas.Items.IndexOf(
            this.ListMonsterArenaAreas.SelectedItem
        );
        var monsters = MonsterArenaData.MonsterArenaAreas[selectedArea].Monsters;

        for (var i = 0; i < monsters.Length; i++)
        {
            var index = monsters[i].Index;
            var count = arenaBytes[index];

            var column = i > 6 ? 1 : 0;
            var row = i > 6 ? i - 7 : i;

            var monsterName = new TextBlock()
            {
                Text = monsters[i].Name,
                VerticalAlignment = VerticalAlignment.Center,
            };
            var monsterCount = new TextBox()
            {
                Text = count.ToString(),
                Width = 40,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
            };
            var monsterPanel = new DockPanel
            {
                Children = { monsterName, monsterCount },
                Margin = new Thickness(5),
            };

            monsterCount.KeyDown += (sender, args) =>
            {
                if (args.Key != Key.Enter)
                {
                    return;
                }

                try
                {
                    var textBox = sender as TextBox;
                    var panelIndex = this.GridMonsterArenaMonsters.Children.IndexOf(
                        (DockPanel)textBox.Parent
                    );
                    var monsterIndex = monsters[panelIndex].Index;
                    this.WriteMonster(monsterIndex, byte.Parse(monsterCount.Text));
                    this.Refresh();
                    textBox.SelectAll();
                }
                catch (Exception)
                {
                    Error.Show("Please enter a value between 0 and 255");
                }
            };

            Grid.SetColumn(monsterPanel, column);
            Grid.SetRow(monsterPanel, row);

            this.GridMonsterArenaMonsters.Children.Add(monsterPanel);
        }
    }

    void WriteMonster(int index, byte count) => MonsterArena.SetCaptureCount(index, count);

    void ListMonsterArenaAreas_OnSelectedItemChanged(
        object sender,
        RoutedPropertyChangedEventArgs<object> e
    )
    {
        var index = this.ListMonsterArenaAreas.Items.IndexOf(
            this.ListMonsterArenaAreas.SelectedItem
        );
        var areaName = MonsterArenaData.MonsterArenaAreas[index].Name;
        this.GroupMonsters.Header = areaName;
        this.Refresh();
    }

    void ButtonCaptureAll_Click(object sender, RoutedEventArgs e)
    {
        for (var i = 0; i < MonsterArenaData.MonsterArenaAreas.Length; i++)
        {
            for (var j = 0; j < MonsterArenaData.MonsterArenaAreas[i].Monsters.Length; j++)
            {
                MonsterArena.SetCaptureCount(
                    MonsterArenaData.MonsterArenaAreas[i].Monsters[j].Index,
                    i > 12 ? (byte)1 : (byte)10
                );
            }
        }

        this.Refresh();
    }

    void ButtonReleaseAll_Click(object sender, RoutedEventArgs e)
    {
        for (var i = 0; i < MonsterArenaData.MonsterArenaAreas.Length; i++)
        {
            for (var j = 0; j < MonsterArenaData.MonsterArenaAreas[i].Monsters.Length; j++)
            {
                MonsterArena.SetCaptureCount(
                    MonsterArenaData.MonsterArenaAreas[i].Monsters[j].Index,
                    0
                );
            }
        }

        this.Refresh();
    }
}
