using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Farplane.Common;
using Farplane.FFX.Data;
using Farplane.FFX.Values;

namespace Farplane.FFX.EditorPanels.MonsterArenaPanel;

public partial class MonsterArenaPanel : UserControl
{
    bool _refreshing;

    public MonsterArenaPanel()
    {
        this.InitializeComponent();

        foreach (var area in MonsterArenaData.MonsterArenaAreas)
        {
            this.ListMonsterArenaAreas.Items.Add(new TreeViewItem { Header = area.Name });
        }
        if (this.ListMonsterArenaAreas.Items.Count > 0)
        {
            ((TreeViewItem)this.ListMonsterArenaAreas.Items[0]).IsSelected = true;
        }
    }

    public void Refresh()
    {
        if (this.ListMonsterArenaAreas.SelectedItem == null)
        {
            return;
        }

        this._refreshing = true;
        this.GridMonsterArenaMonsters.Children.Clear();
        var arenaBytes = MonsterArena.GetCaptureCounts();
        var selectedArea = this.ListMonsterArenaAreas.Items.IndexOf(
            this.ListMonsterArenaAreas.SelectedItem
        );

        if (selectedArea < 0 || selectedArea >= MonsterArenaData.MonsterArenaAreas.Length)
        {
            this._refreshing = false;
            return;
        }

        var areaData = MonsterArenaData.MonsterArenaAreas[selectedArea];
        var monsters = areaData.Monsters;
        var isCreationArea = selectedArea >= 13; // Area Conquest (13), Species Conquest (14), Original (15)

        for (var i = 0; i < monsters.Length; i++)
        {
            var monster = monsters[i];
            var monsterIndex = monster.Index;
            var column = i > 6 ? 1 : 0;
            var row = i > 6 ? i - 7 : i;

            if (isCreationArea)
            {
                // Creation Area: render CheckBox for unlock state (0 = locked, >0 = unlocked)
                var isUnlocked = monsterIndex < arenaBytes.Length && arenaBytes[monsterIndex] != 0;
                var checkBox = new CheckBox
                {
                    Content = monster.Name,
                    IsChecked = isUnlocked,
                    Tag = monsterIndex,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(4, 2, 4, 2)
                };

                checkBox.Checked += this.CreationCheckBox_Changed;
                checkBox.Unchecked += this.CreationCheckBox_Changed;

                Grid.SetColumn(checkBox, column);
                Grid.SetRow(checkBox, row);
                this.GridMonsterArenaMonsters.Children.Add(checkBox);
            }
            else
            {
                // Regular Area: render Label + TextBox for capture count (0..10)
                var count = monsterIndex < arenaBytes.Length ? arenaBytes[monsterIndex] : (byte)0;

                var monsterName = new TextBlock
                {
                    Text = monster.Name,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    ToolTip = monster.Name
                };

                var monsterCount = new TextBox
                {
                    Text = count.ToString(),
                    Width = 40,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                    Tag = monsterIndex
                };

                monsterCount.KeyDown += (sender, args) =>
                {
                    if (args.Key != Key.Enter)
                    {
                        return;
                    }

                    try
                    {
                        var textBox = (TextBox)sender;
                        var idx = (int)textBox.Tag;
                        var newCount = byte.Parse(textBox.Text);
                        MonsterArena.SetCaptureCount(idx, newCount);
                        this.Refresh();
                        textBox.SelectAll();
                    }
                    catch
                    {
                        Error.Show("Please enter a value between 0 and 255.");
                    }
                };

                var monsterPanel = new DockPanel
                {
                    Children = { monsterName, monsterCount },
                    Margin = new Thickness(4, 2, 4, 2)
                };

                Grid.SetColumn(monsterPanel, column);
                Grid.SetRow(monsterPanel, row);
                this.GridMonsterArenaMonsters.Children.Add(monsterPanel);
            }
        }

        this._refreshing = false;
    }

    void CreationCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (this._refreshing)
        {
            return;
        }

        if (sender is CheckBox checkBox && checkBox.Tag is int index)
        {
            var isUnlocked = checkBox.IsChecked == true;
            MonsterArena.SetCreationUnlocked(index, isUnlocked);
        }
    }

    void ListMonsterArenaAreas_OnSelectedItemChanged(
        object sender,
        RoutedPropertyChangedEventArgs<object> e
    )
    {
        var index = this.ListMonsterArenaAreas.Items.IndexOf(
            this.ListMonsterArenaAreas.SelectedItem
        );

        if (index >= 0 && index < MonsterArenaData.MonsterArenaAreas.Length)
        {
            this.GroupMonsters.Header = MonsterArenaData.MonsterArenaAreas[index].Name;
            this.Refresh();
        }
    }

    void ButtonCaptureAll_Click(object sender, RoutedEventArgs e)
    {
        MonsterArena.CaptureAllMonsters(10);
        MonsterArena.UnlockAllCreations();
        this.Refresh();
    }

    void ButtonReleaseAll_Click(object sender, RoutedEventArgs e)
    {
        MonsterArena.ReleaseAllMonsters();
        MonsterArena.LockAllCreations();
        this.Refresh();
    }

    void ButtonUnlockAllCreations_Click(object sender, RoutedEventArgs e)
    {
        MonsterArena.UnlockAllCreations();
        this.Refresh();
    }

    void ButtonLockAllCreations_Click(object sender, RoutedEventArgs e)
    {
        MonsterArena.LockAllCreations();
        this.Refresh();
    }
}
