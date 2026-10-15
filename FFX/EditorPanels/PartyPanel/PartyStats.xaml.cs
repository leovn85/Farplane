using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Farplane.FFX.Data;
using Farplane.Memory;

namespace Farplane.FFX.EditorPanels.PartyPanel;

/// <summary>
/// Interaction logic for PartyStats.xaml
/// </summary>
public partial class PartyStats : UserControl
{
    int _offsetPartyStats => OffsetScanner.GetOffset(GameOffset.FFX_PartyStatBase);
    readonly int _blockSize = Marshal.SizeOf<PartyMember>();
    int _characterIndex;
    bool _canWriteData;

    public PartyStats() => this.InitializeComponent();

    public void Refresh(int characterIndex)
    {
        this._canWriteData = false;
        this._characterIndex = characterIndex;
        var partyMember = Party.ReadPartyMember(this._characterIndex);
        this.TextTotalAP.Text = partyMember.ApTotal.ToString();
        this.TextCurrentAP.Text = partyMember.ApCurrent.ToString();
        this.TextCurrentHP.Text = partyMember.CurrentHp.ToString();
        this.TextCurrentMP.Text = partyMember.CurrentMp.ToString();
        this.TextMaxHP.Text = partyMember.CurrentHpMax.ToString();
        this.TextMaxMP.Text = partyMember.CurrentMpMax.ToString();
        this.TextBaseHP.Text = partyMember.BaseHp.ToString();
        this.TextBaseMP.Text = partyMember.BaseMp.ToString();
        this.TextSphereLevelCurrent.Text = partyMember.SphereLevelCurrent.ToString();
        this.TextSphereLevelTotal.Text = partyMember.SphereLevelTotal.ToString();
        var inParty = partyMember.InParty;
        var partyComboIndex = 0;
        switch (inParty)
        {
            case 0:
                partyComboIndex = 2;
                break;
            case 16:
                partyComboIndex = 1;
                break;
            case 17:
                partyComboIndex = 0;
                break;
        }
        this.ComboInParty.SelectedIndex = partyComboIndex;
        this.TextBaseStrength.Text = partyMember.BaseStrength.ToString();
        this.TextBaseDefense.Text = partyMember.BaseDefense.ToString();
        this.TextBaseMagic.Text = partyMember.BaseMagic.ToString();
        this.TextBaseMagicDef.Text = partyMember.BaseMagicDefense.ToString();
        this.TextBaseAgility.Text = partyMember.BaseAgility.ToString();
        this.TextBaseLuck.Text = partyMember.BaseLuck.ToString();
        this.TextBaseEvasion.Text = partyMember.BaseEvasion.ToString();
        this.TextBaseAccuracy.Text = partyMember.BaseAccuracy.ToString();
        this._canWriteData = true;
    }

    void TextBox_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }
        try
        {
            var offset = 0;
            var textBox = sender as TextBox;
            var charOffset = this._offsetPartyStats + (this._characterIndex * this._blockSize);
            switch (textBox.Name)
            {
                case "TextTotalAP":
                    offset =
                        (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.ApTotal))
                        + charOffset;
                    GameMemory.Write(offset, uint.Parse(this.TextTotalAP.Text), false);
                    break;
                case "TextCurrentAP":
                    offset =
                        (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.ApCurrent))
                        + charOffset;
                    GameMemory.Write(offset, uint.Parse(this.TextCurrentAP.Text), false);
                    break;
                case "TextMaxHP":
                    offset =
                        (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.CurrentHpMax))
                        + charOffset;
                    GameMemory.Write(offset, uint.Parse(this.TextMaxHP.Text), false);
                    break;
                case "TextMaxMP":
                    offset =
                        (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.CurrentMpMax))
                        + charOffset;
                    GameMemory.Write(offset, uint.Parse(this.TextMaxMP.Text), false);
                    break;
                case "TextCurrentHP":
                    offset =
                        (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.CurrentHp))
                        + charOffset;
                    GameMemory.Write(offset, uint.Parse(this.TextCurrentHP.Text), false);
                    break;
                case "TextCurrentMP":
                    offset =
                        (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.CurrentMp))
                        + charOffset;
                    GameMemory.Write(offset, uint.Parse(this.TextCurrentMP.Text), false);
                    break;
                case "TextBaseHP":
                    offset =
                        (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.BaseHp)) + charOffset;
                    GameMemory.Write(offset, uint.Parse(this.TextBaseHP.Text), false);
                    break;
                case "TextBaseMP":
                    offset =
                        (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.BaseMp)) + charOffset;
                    GameMemory.Write(offset, uint.Parse(this.TextBaseMP.Text), false);
                    break;
                case "TextSphereLevelCurrent":
                    offset =
                        (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.SphereLevelCurrent))
                        + charOffset;
                    GameMemory.Write(offset, byte.Parse(this.TextSphereLevelCurrent.Text), false);
                    break;
                case "TextSphereLevelTotal":
                    offset =
                        (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.SphereLevelTotal))
                        + charOffset;
                    GameMemory.Write(offset, byte.Parse(this.TextSphereLevelTotal.Text), false);
                    break;
                case "TextBaseStrength":
                    offset =
                        (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.BaseStrength))
                        + charOffset;
                    GameMemory.Write(offset, byte.Parse(this.TextBaseStrength.Text), false);
                    break;
                case "TextBaseDefense":
                    offset =
                        (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.BaseDefense))
                        + charOffset;
                    GameMemory.Write(offset, byte.Parse(this.TextBaseDefense.Text), false);
                    break;
                case "TextBaseMagic":
                    offset =
                        (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.BaseMagic))
                        + charOffset;
                    GameMemory.Write(offset, byte.Parse(this.TextBaseMagic.Text), false);
                    break;
                case "TextBaseMagicDef":
                    offset =
                        (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.BaseMagicDefense))
                        + charOffset;
                    GameMemory.Write(offset, byte.Parse(this.TextBaseMagicDef.Text), false);
                    break;
                case "TextBaseAgility":
                    offset =
                        (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.BaseAgility))
                        + charOffset;
                    GameMemory.Write(offset, byte.Parse(this.TextBaseAgility.Text), false);
                    break;
                case "TextBaseLuck":
                    offset =
                        (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.BaseLuck))
                        + charOffset;
                    GameMemory.Write(offset, byte.Parse(this.TextBaseLuck.Text), false);
                    break;
                case "TextBaseEvasion":
                    offset =
                        (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.BaseEvasion))
                        + charOffset;
                    GameMemory.Write(offset, byte.Parse(this.TextBaseEvasion.Text), false);
                    break;
                case "TextBaseAccuracy":
                    offset =
                        (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.BaseAccuracy))
                        + charOffset;
                    GameMemory.Write(offset, byte.Parse(this.TextBaseAccuracy.Text), false);
                    break;
            }
            textBox.SelectAll();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"An error occurred:\n{ex.Message}",
                "Error parsing input",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
            return;
        }
    }

    void ComboInParty_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!this._canWriteData)
        {
            return;
        }
        byte partyState = 0;
        switch (this.ComboInParty.SelectedIndex)
        {
            case 0:
                partyState = 17;
                break;
            case 1:
                partyState = 16;
                break;
            case 2:
                partyState = 0;
                break;
        }
        var charOffset = this._offsetPartyStats + (this._characterIndex * this._blockSize);
        var offset = (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.InParty)) + charOffset;
        GameMemory.Write(offset, partyState, false);
        this.Refresh(this._characterIndex);
    }
}
