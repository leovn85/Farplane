using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;

namespace Farplane.Common.Dialogs;

/// <summary>
/// Interaction logic for CommandSelectDialog.xaml
/// </summary>
public partial class SearchDialog : MetroWindow
{
    readonly List<string> _searchList = [];
    readonly List<string> _searchResults = [];
    string _lastSearch = string.Empty;

    public int ResultIndex = -1;

    public SearchDialog(
        List<string> searchList,
        string defaultSearch = "",
        bool showNoSelection = true
    )
    {
        this.InitializeComponent();

        this.ButtonClearSlot.Visibility = showNoSelection
            ? Visibility.Visible
            : Visibility.Collapsed;

        this._searchList = searchList;

        this.ListCommandSearch.ItemsSource = this._searchResults;

        if (defaultSearch != "")
        {
            this.PerformSearch(defaultSearch);
        }

        this._lastSearch = int.MaxValue.ToString();

        this.TextSearchBox.Focus();
        this.PerformSearch("");
    }

    public void PerformSearch(string searchString)
    {
        var lCaseSearch = searchString.ToLower();

        var result = this._searchList.FindAll(item => item.ToLower().Contains(lCaseSearch));
        if (result.Count == 0)
        {
            return;
        }

        this._searchResults.Clear();
        foreach (var s in result)
        {
            this._searchResults.Add(s);
        }

        this.ListCommandSearch.Items.Refresh();
        if (this._searchResults.Count > 0)
        {
            this.ListCommandSearch.SelectedIndex = 0;
        }

        this.TextSearchBox.Text = searchString;
        this.TextSearchBox.SelectionStart = 0;
        this.TextSearchBox.SelectionLength = searchString.Length;
        this.TextSearchBox.Focus();
    }

    void TextCommandSearch_OnKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                // Confirm choice if pressed twice
                if (this._lastSearch == this.TextSearchBox.Text)
                {
                    this.TryCloseAndReturn();
                }
                // Execute search
                this.PerformSearch(this.TextSearchBox.Text);
                this._lastSearch = this.TextSearchBox.Text;
                break;
            case Key.Escape:
                this.Close();
                return;
            default:
                return;
        }
    }

    void ButtonSelectedCommand_OnClick(object sender, RoutedEventArgs e) =>
        this.TryCloseAndReturn();

    void TryCloseAndReturn()
    {
        if (this.ListCommandSearch.SelectedItems.Count != 1)
        {
            return;
        }

        var searchIndex = this._searchList.IndexOf(this.ListCommandSearch.SelectedItem.ToString());
        this.ResultIndex = searchIndex;
        this.DialogResult = true;
        this.Close();
    }

    void ButtonCancelSelection_OnClick(object sender, RoutedEventArgs e)
    {
        this.DialogResult = false;
        this.Close();
    }

    void ListCommandSearch_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) =>
        this.TryCloseAndReturn();

    void ButtonClearSlot_Click(object sender, RoutedEventArgs e)
    {
        this.ResultIndex = -1;
        this.DialogResult = true;
        this.Close();
    }
}
