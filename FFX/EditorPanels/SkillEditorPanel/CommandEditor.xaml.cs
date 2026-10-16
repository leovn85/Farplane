using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Farplane.Common;
using Farplane.FFX.Data;
using Microsoft.Win32;

namespace Farplane.FFX.EditorPanels.SkillEditorPanel;

public partial class CommandEditor : UserControl
{
    KernelType _currentType = KernelType.Command;
    List<CommandListItem> _allItems = [];
    bool _refreshing;


    bool _ready = false;

    public CommandEditor()
    {
        this.InitializeComponent();
        this._ready = true;
    }

    public void Refresh()
    {
        if (!this._ready)
        {
            return;
        }

        this._refreshing = true;
        try
        {
            Skills.Reset();
            this._allItems.Clear();
            var names = Skills.GetNames(this._currentType);

            if (names != null)
            {
                for (var i = 0; i < names.Count; i++)
                {
                    var name = string.IsNullOrWhiteSpace(names[i]) ? "<Unnamed Action>" : names[i];
                    this._allItems.Add(new CommandListItem { Index = i, Name = name });
                }
            }

            this.ApplyFilter();

            if (this.ListCommands != null && this.ListCommands.Items.Count > 0 && this.ListCommands.SelectedIndex == -1)
            {
                this.ListCommands.SelectedIndex = 0;
            }
        }
        finally
        {
            this._refreshing = false;
        }
    }

    void ApplyFilter()
    {
        if (this.TextSearch == null || this.ListCommands == null)
        {
            return;
        }

        var filter = this.TextSearch.Text?.Trim().ToLower() ?? string.Empty;
        var filtered = string.IsNullOrEmpty(filter)
            ? this._allItems
            : this._allItems.Where(item => item.Index.ToString().Contains(filter) || item.Name.ToLower().Contains(filter)).ToList();

        this.ListCommands.ItemsSource = filtered;

        if (filtered.Count > 0)
        {
            this.ListCommands.SelectedIndex = 0;
        }
    }

    void TextSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!this._refreshing)
        {
            this.ApplyFilter();
        }
    }

    void ComboTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!this._ready)
        {
            return;
        }

        this._currentType = this.ComboTable.SelectedIndex == 1 ? KernelType.Item : KernelType.Command;
        this.Refresh();
    }

    void ListCommands_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (this.ListCommands.SelectedItem is not CommandListItem selectedItem)
        {
            return;
        }

        var index = selectedItem.Index;
        var name = Skills.GetSkillName(index, this._currentType);
        var desc = Skills.GetSkillDescription(index, this._currentType);

        this.TextCommandName.Text = $"[{index}] {name}";
        this.Description.Text = string.IsNullOrWhiteSpace(desc) ? "No description available." : desc;
        this.GroupHeader.Header = $"[{index}] {name}";

        this.SkillDataPanel.LoadCommand(index, this._currentType);
    }

    void ButtonExport_Click(object sender, RoutedEventArgs e)
    {
        var table = Skills.GetTable(this._currentType);
        if (table == null || table.BufferAddress == 0)
        {
            Error.Show("Cannot export: Game kernel table is not loaded in memory.");
            return;
        }

        var buffer = table.ExportBuffer();
        if (buffer.Length == 0)
        {
            Error.Show("Kernel buffer is empty.");
            return;
        }

        var defaultName = this._currentType == KernelType.Command ? "command.bin" : "item.bin";
        var sfd = new SaveFileDialog
        {
            FileName = defaultName,
            Filter = "Kernel Binary (*.bin)|*.bin|All Files (*.*)|*.*",
            Title = "Export In-Memory Kernel Table"
        };

        if (sfd.ShowDialog() == true)
        {
            File.WriteAllBytes(sfd.FileName, buffer);
            MessageBox.Show($"Exported {buffer.Length:N0} bytes to:\n{sfd.FileName}", "Export Completed", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    void ButtonHotReload_Click(object sender, RoutedEventArgs e)
    {
        var table = Skills.GetTable(this._currentType);
        if (table == null || table.BufferAddress == 0)
        {
            Error.Show("Cannot hot-reload: Game memory is not hooked.");
            return;
        }

        var ofd = new OpenFileDialog
        {
            Filter = "Kernel Binary (*.bin)|*.bin|All Files (*.*)|*.*",
            Title = "Hot-Reload Kernel Table into RAM"
        };

        if (ofd.ShowDialog() != true)
        {
            return;
        }

        var bytes = File.ReadAllBytes(ofd.FileName);
        if (table.HotReload(bytes, out var error))
        {
            this.Refresh();
            MessageBox.Show($"Hot-reload successful!\nInjected {bytes.Length:N0} bytes into game memory.", "Kernel Reloaded", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            Error.Show($"Hot-reload failed:\n{error}");
        }
    }

    public class CommandListItem
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public override string ToString() => $"[{this.Index:D3}] {this.Name}";
    }
}
