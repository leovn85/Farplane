using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Farplane.Common.Dialogs;
using Farplane.FFX2.Values;
using MahApps.Metro.Controls;

namespace Farplane.FFX2.EditorPanels;

/// <summary>
/// Interaction logic for CreatureAbilities.xaml
/// </summary>
public partial class CreatureAbilities : UserControl
{
    readonly int _abilityOffset = (int)OffsetType.CreatureAbilities;

    readonly int _creatureIndex = 0;

    public CreatureAbilities(int creatureIndex)
    {
        this._creatureIndex = creatureIndex;

        this.InitializeComponent();
        this.Refresh();
    }

    public void Refresh()
    {
        var creatureBytes = LegacyMemoryReader.ReadBytes(
            this._abilityOffset + (this._creatureIndex * 0xE38),
            0x16
        );

        // Refresh commands
        for (var commandSlot = 0; commandSlot < 8; commandSlot++)
        {
            var commandId = BitConverter.ToUInt16(creatureBytes, commandSlot * 2);
            var commandButton = (Button)this.FindName("Command" + commandSlot);
            if (commandButton == null)
            {
                return;
            }

            if (commandId is 0xFF or 0x0)
            {
                commandButton.Content = string.Empty;
            }
            else
            {
                var command = Commands.GetCommand(commandId);
                if (command != null)
                {
                    this.SetCommandText(commandButton, command);
                }
                else
                {
                    var auto = AutoAbilities.GetAutoAbility(commandId);
                    this.SetAutoAbilityText(commandButton, auto);
                }
            }
        }
    }

    void CommandButton_Click(object sender, RoutedEventArgs e)
    {
        this.Refresh();
        var senderButton = (Button)sender;
        if (senderButton == null)
        {
            return;
        }

        var commandIndex = int.Parse(senderButton.Name.Substring(7));

        // ABILITY SEARCH

        var searchList = this.BuildSearchList();
        var textList = new List<string>();
        foreach (var item in searchList)
        {
            textList.Add($"{item.ID:X4} {item.Name}");
        }

        var currentCmd = this.ReadAbility(commandIndex);
        var commandDialog = new SearchDialog(textList, currentCmd.ToString("X2"))
        {
            Owner = this.TryFindParent<Window>(),
        };

        commandDialog.ShowDialog();
        if (commandDialog.DialogResult.HasValue && !commandDialog.DialogResult.Value)
        {
            return;
        }

        var searchIndex = commandDialog.ResultIndex;

        if (searchIndex == -1)
        {
            this.WriteAbility(commandIndex, 0xFF);
        }
        else
        {
            var searchCommand = searchList[searchIndex];
            this.WriteAbility(commandIndex, searchCommand.ID);
        }
    }

    void WriteAbility(int index, int abilityId)
    {
        LegacyMemoryReader.WriteBytes(
            this._abilityOffset + (this._creatureIndex * 0xE38) + (index * 2),
            BitConverter.GetBytes((ushort)abilityId)
        );
        this.Refresh();
    }

    ushort ReadAbility(int index)
    {
        var creatureBytes = LegacyMemoryReader.ReadBytes(
            this._abilityOffset + (this._creatureIndex * 0xE38),
            0x16
        );
        var abilityId = BitConverter.ToUInt16(creatureBytes, index * 2);
        return abilityId;
    }

    void ShowButtonBox(Button button, string defaultText)
    {
        this.Refresh();

        var inputBox = new TextBox
        {
            Text = defaultText,
            SelectionStart = 0,
            SelectionLength = defaultText.Length,
            ContextMenu = null,
        };
        button.Content = inputBox;
        button.UpdateLayout();
        inputBox.Focus();
        inputBox.KeyDown += (sender, args) =>
        {
            if (args.Key == Key.Escape)
            {
                // Cancel input
                this.Refresh();
                return;
            }

            if (args.Key != Key.Enter)
            {
                return;
            }

            // Attempt to parse command ID and write ability
            var commandIndex = int.Parse(button.Name.Substring(7));
            var commandId = -1;

            var foundId = int.TryParse(
                inputBox.Text,
                NumberStyles.HexNumber,
                CultureInfo.InvariantCulture,
                out commandId
            );

            if (!foundId)
            {
                return;
            }

            this.WriteAbility(commandIndex, commandId);
        };
    }

    void SetAutoAbilityText(Button button, AutoAbility ability)
    {
        if (ability != null)
        {
            button.Content = $"[{ability.ID:X4}] {ability.Name}";
        }
        else
        {
            button.Content = $"Unknown ID";
        }
    }

    void SetCommandText(Button button, Command command)
    {
        if (command != null)
        {
            button.Content = $"[{command.ID:X4}] {command.Name}";
        }
        else
        {
            button.Content = $"[{command.ID:X4}] ????";
        }
    }

    List<AbilitySearchItem> BuildSearchList()
    {
        var searchList = new List<AbilitySearchItem>();

        foreach (var command in Commands.CommandList)
        {
            searchList.Add(
                new AbilitySearchItem
                {
                    ID = command.ID,
                    Name = command.Name,
                    Type = AbilitySearchItem.AbilityType.Command,
                }
            );
        }

        foreach (var command in AutoAbilities.AutoAbilityList)
        {
            searchList.Add(
                new AbilitySearchItem
                {
                    ID = command.ID,
                    Name = command.Name,
                    Type = AbilitySearchItem.AbilityType.AutoAbility,
                }
            );
        }

        return searchList;

        //var searchList = new List<string>();
        //foreach (var command in Commands.CommandList)
        //    searchList.Add($"{command.ID.ToString("X2")} {command.Name}");
        //foreach (var command in AutoAbilities.AutoAbilityList)
        //    searchList.Add($"{command.ID.ToString("X2")} {command.Name}");
    }

    void CommandButton_RightMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Button senderButton)
        {
            return;
        }

        var buttonIndex = int.Parse(senderButton.Name.Substring(7));
        var abilityId = LegacyMemoryReader.ReadInt16(
            this._abilityOffset + (0xE38 * this._creatureIndex) + (2 * buttonIndex)
        );
        this.ShowButtonBox(senderButton, abilityId.ToString("X2"));
    }

    class AbilitySearchItem
    {
        public AbilityType Type { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }

        public enum AbilityType
        {
            Command,
            AutoAbility,
        }
    }
}
