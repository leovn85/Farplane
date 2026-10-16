using System;
using System.Threading;
using System.Windows;
using Farplane.FFX.Values;
using Farplane.Memory;
using CheckBox = System.Windows.Controls.CheckBox;

namespace Farplane.FFX.EditorPanels.Boosters;

/// <summary>
/// Interaction logic for BoostersPanel.xaml
/// </summary>
public partial class BoostersPanel
{
    static int _offsetInBattle => OffsetScanner.GetOffset(GameOffset.FFX_InBattleFlags);
    static int _offsetGainedAp => OffsetScanner.GetOffset(GameOffset.FFX_GainedAPFlags);
    readonly Thread _apThread;
    readonly CancellationTokenSource _cts = new();
    readonly bool[] _shareApChecked = new bool[8];
    bool _sharedApEnabled;

    public BoostersPanel()
    {
        this.InitializeComponent();

        for (var i = 0; i < 8; i++)
        {
            var isChecked = i != 7;
            this._shareApChecked[i] = isChecked;
            var boxIndex = i;

            var checkBox = new CheckBox()
            {
                Name = "CheckBoxAPShare" + i,
                Content = (Character)i,
                Margin = new Thickness(5),
                IsChecked = isChecked,
            };
            checkBox.Checked += (s, e) => this._shareApChecked[boxIndex] = true;
            checkBox.Unchecked += (s, e) => this._shareApChecked[boxIndex] = false;
            this.ShareBoxes.Children.Add(checkBox);
        }

        this._apThread = new Thread(this.SharedAPThread) { IsBackground = true };
        this._apThread.Start();
    }

    public void Stop()
    {
        try
        {
            this._cts.Cancel();
        }
        catch { }
    }

    public void Refresh()
    {
        // No refresh logic for this panel
    }

    void GiveAllItems_Click(object sender, RoutedEventArgs e) => Cheats.GiveAllItems();
    void MaxAllStats_Click(object sender, RoutedEventArgs e) => Cheats.MaxAllStats();
    void MaxSphereLevels_Click(object sender, RoutedEventArgs e) => Cheats.MaxSphereLevels();
    void LearnAllAbilities_Click(object sender, RoutedEventArgs e) => Cheats.LearnAllAbilities();

    void SharedAPToggle_Click(object sender, RoutedEventArgs e)
    {
        this._sharedApEnabled = !this._sharedApEnabled;
        this.ButtonSharedAP.Content = this._sharedApEnabled ? "ENABLED" : "DISABLED";
    }

    void SharedAPThread()
    {
        while (!this._cts.Token.IsCancellationRequested)
        {
            if (this._sharedApEnabled && GameMemory.IsAttached)
            {
                var offsetInBattle = _offsetInBattle;
                var offsetGainedAp = _offsetGainedAp;
                if (offsetInBattle != 0 && offsetGainedAp != 0)
                {
                    // Read and compute on background thread without Dispatcher.Invoke
                    var gainedAp = GameMemory.Read<byte>(offsetGainedAp, 8, false);
                    var writeBuffer = new byte[8];
                    for (var i = 0; i < 8; i++)
                    {
                        writeBuffer[i] = this._shareApChecked[i]
                            ? (byte)1
                            : (gainedAp != null ? gainedAp[i] : (byte)0);
                    }
                    GameMemory.Write(offsetInBattle, writeBuffer, false);
                    GameMemory.Write(offsetGainedAp, writeBuffer, false);
                }
            }
            Thread.Sleep(10);
        }
    }
}
