using System;
using System.Windows.Controls;
using System.Windows.Input;
using Farplane.Common;
using Farplane.FFX.Values;

namespace Farplane.FFX.EditorPanels.GeneralPanel;

/// <summary>
/// Interaction logic for GeneralPanel.xaml
/// </summary>
public partial class GeneralPanel : UserControl
{
    readonly Character[] _enumValues = (Character[])Enum.GetValues(typeof(Character));
    bool _refreshing = false;

    public GeneralPanel()
    {
        this.InitializeComponent();

        this._refreshing = true;
        var characterList = Enum.GetNames(typeof(Character));
        foreach (ComboBox comboBox in this.StackCurrentParty.Children)
        {
            comboBox.ItemsSource = characterList;
        }
        this._refreshing = false;
    }

    public void Refresh()
    {
        this._refreshing = true;

        this.TextGil.Text = Data.General.CurrentGil.ToString();
        this.TextTidusOverdrive.Text = Data.General.TidusOverdrive.ToString();

        var partyList = Data.Party.GetActiveParty();
        for (var i = 0; i < 8; i++)
        {
            var comboBox = this.StackCurrentParty.Children[i] as ComboBox;
            comboBox.SelectedIndex = Array.IndexOf(this._enumValues, partyList[i]);
        }

        this._refreshing = false;
    }

    void TextGil_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        try
        {
            var currentGil = int.Parse(this.TextGil.Text);
            Data.General.CurrentGil = currentGil;
            this.TextGil.SelectAll();
        }
        catch
        {
            Error.Show("The value you entered was invalid.");
        }
    }

    void TextTidusOverdrive_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        try
        {
            var tidusOverdrive = int.Parse(this.TextTidusOverdrive.Text);
            Data.General.TidusOverdrive = tidusOverdrive;
            this.TextTidusOverdrive.SelectAll();
        }
        catch
        {
            Error.Show("The value you entered was invalid.");
        }
    }

    void PartyMember_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (this._refreshing)
        {
            return;
        }

        var charArray = new Character[8];
        for (var i = 0; i < 8; i++)
        {
            var comboBox = this.StackCurrentParty.Children[i] as ComboBox;
            charArray[i] = this._enumValues[comboBox.SelectedIndex];
        }
        Data.Party.SetActiveParty(charArray);
    }
}
