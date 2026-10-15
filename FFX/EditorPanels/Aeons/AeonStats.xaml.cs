using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Farplane.Common;
using Farplane.FFX.Data;
using Farplane.FFX.Values;
using Farplane.Memory;

namespace Farplane.FFX.EditorPanels.Aeons;

/// <summary>
/// Interaction logic for PartyStats.xaml
/// </summary>
public partial class AeonStats : UserControl
{
    int _characterIndex;

    // Default stats for aeons when unlocking them early (roughly matching normal obtain-time stats)
    static readonly Dictionary<Character, AeonDefaults> _aeonDefaults =
        new()
        {
            {
                Character.Valefor,
                new AeonDefaults("Valefor", 1030, 42, 26, 22, 30, 24, 14, 8, 12, 30, 20)
            },
            {
                Character.Ifrit,
                new AeonDefaults("Ifrit", 1440, 20, 31, 28, 24, 20, 11, 7, 8, 28, 20)
            },
            {
                Character.Ixion,
                new AeonDefaults("Ixion", 2080, 68, 28, 24, 32, 26, 16, 8, 14, 32, 20)
            },
            {
                Character.Shiva,
                new AeonDefaults("Shiva", 2560, 84, 26, 22, 34, 28, 18, 8, 16, 34, 20)
            },
            {
                Character.Bahamut,
                new AeonDefaults("Bahamut", 5200, 128, 40, 36, 38, 32, 20, 12, 18, 38, 20)
            },
            {
                Character.Yojimbo,
                new AeonDefaults("Yojimbo", 3400, 0, 36, 32, 22, 20, 18, 10, 16, 36, 20)
            },
            {
                Character.Anima,
                new AeonDefaults("Anima", 6800, 96, 48, 40, 46, 38, 18, 10, 14, 46, 20)
            },
            {
                Character.Cindy,
                new AeonDefaults("Cindy", 4800, 180, 30, 28, 40, 36, 16, 10, 14, 30, 20)
            },
            {
                Character.Sandy,
                new AeonDefaults("Sandy", 4500, 92, 44, 38, 32, 28, 20, 10, 18, 44, 20)
            },
            {
                Character.Mindy,
                new AeonDefaults("Mindy", 3200, 120, 38, 22, 36, 24, 28, 14, 24, 38, 20)
            },
        };

    public AeonStats() => this.InitializeComponent();

    public void Refresh(int characterIndex)
    {
        this._characterIndex = characterIndex;
        var partyMember = Party.ReadPartyMember(this._characterIndex);

        // Update obtained button state
        var isObtained = partyMember.InParty >= 0x10;
        this.ButtonObtained.Content = isObtained ? "OBTAINED" : "NOT OBTAINED";

        this.TextAeonName.Text = AeonName.GetName(characterIndex);

        this.TextBaseHP.Text = partyMember.BaseHp.ToString();
        this.TextBaseMP.Text = partyMember.BaseMp.ToString();

        this.TextOverdrive.Text = partyMember.OverdriveLevel.ToString();
        this.TextOverdriveMax.Text = partyMember.OverdriveMax.ToString();

        this.TextBaseStrength.Text = partyMember.BaseStrength.ToString();
        this.TextBaseDefense.Text = partyMember.BaseDefense.ToString();
        this.TextBaseMagic.Text = partyMember.BaseMagic.ToString();
        this.TextBaseMagicDef.Text = partyMember.BaseMagicDefense.ToString();
        this.TextBaseAgility.Text = partyMember.BaseAgility.ToString();
        this.TextBaseLuck.Text = partyMember.BaseLuck.ToString();
        this.TextBaseEvasion.Text = partyMember.BaseEvasion.ToString();
        this.TextBaseAccuracy.Text = partyMember.BaseAccuracy.ToString();
    }

    void ButtonObtained_Click(object sender, RoutedEventArgs e)
    {
        var partyMember = Party.ReadPartyMember(this._characterIndex);
        var isObtained = partyMember.InParty >= 0x10;
        var character = (Character)this._characterIndex;

        if (!isObtained)
        {
            // Magus Sisters must be unlocked as a group
            var isMagusSister =
                character == Character.Cindy
                || character == Character.Sandy
                || character == Character.Mindy;

            var unlockIndices = isMagusSister
                ? new[] { (int)Character.Cindy, (int)Character.Sandy, (int)Character.Mindy }
                : new[] { this._characterIndex };

            foreach (var idx in unlockIndices)
            {
                var member = Party.ReadPartyMember(idx);
                if (member.InParty < 0x10)
                {
                    Party.SetPartyMemberAttribute<byte>(idx, nameof(PartyMember.InParty), 0x11);
                }

                var ch = (Character)idx;
                if (member.BaseHp == 0 && _aeonDefaults.TryGetValue(ch, out var defs))
                {
                    UnlockAeonStats(idx, defs);
                }
            }

            var unlockName = isMagusSister ? "Magus Sisters (Cindy, Sandy, Mindy)" : character.ToString();
            MessageBox.Show(
                $"{unlockName} unlocked!\n\n"
                    + "The aeon should now be available in Yuna's Summon menu.",
                "Aeon Unlocked",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
        else
        {
            // Lock the aeon
            var isMagusSister =
                character == Character.Cindy
                || character == Character.Sandy
                || character == Character.Mindy;

            var lockName = isMagusSister ? "Magus Sisters" : character.ToString();
            var result = MessageBox.Show(
                $"Remove {lockName} from obtained aeons?",
                "Confirm",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );
            if (result == MessageBoxResult.Yes)
            {
                var lockIndices = isMagusSister
                    ? new[] { (int)Character.Cindy, (int)Character.Sandy, (int)Character.Mindy }
                    : new[] { this._characterIndex };

                foreach (var idx in lockIndices)
                {
                    Party.SetPartyMemberAttribute<byte>(idx, nameof(PartyMember.InParty), 0x00);
                }
            }
        }

        AeonsPanel.UpdateTabs();
        this.Refresh(this._characterIndex);
    }

    static void UnlockAeonStats(int characterIndex, AeonDefaults defaults)
    {
        AeonName.SetName(characterIndex, defaults.Name);

        var baseOffset = Party.GetMemoryOffset(characterIndex);

        GameMemory.Write(
            baseOffset + (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.BaseHp)),
            defaults.Hp,
            false
        );
        GameMemory.Write(
            baseOffset + (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.BaseMp)),
            defaults.Mp,
            false
        );
        GameMemory.Write(
            baseOffset + (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.CurrentHp)),
            defaults.Hp,
            false
        );
        GameMemory.Write(
            baseOffset + (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.CurrentMp)),
            defaults.Mp,
            false
        );
        GameMemory.Write(
            baseOffset + (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.CurrentHpMax)),
            defaults.Hp,
            false
        );
        GameMemory.Write(
            baseOffset + (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.CurrentMpMax)),
            defaults.Mp,
            false
        );
        GameMemory.Write(
            baseOffset + (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.BaseStrength)),
            defaults.Str,
            false
        );
        GameMemory.Write(
            baseOffset + (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.BaseDefense)),
            defaults.Def,
            false
        );
        GameMemory.Write(
            baseOffset + (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.BaseMagic)),
            defaults.Mag,
            false
        );
        GameMemory.Write(
            baseOffset
                + (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.BaseMagicDefense)),
            defaults.MagDef,
            false
        );
        GameMemory.Write(
            baseOffset + (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.BaseAgility)),
            defaults.Agi,
            false
        );
        GameMemory.Write(
            baseOffset + (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.BaseLuck)),
            defaults.Luck,
            false
        );
        GameMemory.Write(
            baseOffset + (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.BaseEvasion)),
            defaults.Eva,
            false
        );
        GameMemory.Write(
            baseOffset + (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.BaseAccuracy)),
            defaults.Acc,
            false
        );
        GameMemory.Write(
            baseOffset + (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.OverdriveMax)),
            defaults.OdMax,
            false
        );
    }

    void ButtonMaxOverdrive_Click(object sender, RoutedEventArgs e)
    {
        var partyMember = Party.ReadPartyMember(this._characterIndex);
        this.TextOverdrive.Text = partyMember.OverdriveMax.ToString();
        Party.SetPartyMemberAttribute(
            this._characterIndex,
            nameof(PartyMember.OverdriveLevel),
            partyMember.OverdriveMax
        );
        this.Refresh(this._characterIndex);
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
            switch (textBox.Name)
            {
                case "TextOverdrive":
                    Party.SetPartyMemberAttribute(
                        this._characterIndex,
                        nameof(PartyMember.OverdriveLevel),
                        byte.Parse(this.TextOverdrive.Text)
                    );
                    break;
                case "TextOverdriveMax":
                    Party.SetPartyMemberAttribute(
                        this._characterIndex,
                        nameof(PartyMember.OverdriveMax),
                        byte.Parse(this.TextOverdriveMax.Text)
                    );
                    break;
                case "TextAeonName":
                    AeonName.SetName(this._characterIndex, this.TextAeonName.Text);
                    break;
                case "TextBaseHP":
                    offset =
                        (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.BaseHp))
                        + Party.GetMemoryOffset(this._characterIndex);
                    GameMemory.Write(offset, uint.Parse(this.TextBaseHP.Text), false);
                    break;
                case "TextBaseMP":
                    offset =
                        (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.BaseMp))
                        + Party.GetMemoryOffset(this._characterIndex);
                    GameMemory.Write(offset, uint.Parse(this.TextBaseMP.Text), false);
                    break;
                case "TextBaseStrength":
                    offset =
                        (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.BaseStrength))
                        + Party.GetMemoryOffset(this._characterIndex);
                    GameMemory.Write(offset, byte.Parse(this.TextBaseStrength.Text), false);
                    break;
                case "TextBaseDefense":
                    offset =
                        (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.BaseDefense))
                        + Party.GetMemoryOffset(this._characterIndex);
                    GameMemory.Write(offset, byte.Parse(this.TextBaseDefense.Text), false);
                    break;
                case "TextBaseMagic":
                    offset =
                        (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.BaseMagic))
                        + Party.GetMemoryOffset(this._characterIndex);
                    GameMemory.Write(offset, byte.Parse(this.TextBaseMagic.Text), false);
                    break;
                case "TextBaseMagicDef":
                    offset =
                        (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.BaseMagicDefense))
                        + Party.GetMemoryOffset(this._characterIndex);
                    GameMemory.Write(offset, byte.Parse(this.TextBaseMagicDef.Text), false);
                    break;
                case "TextBaseAgility":
                    offset =
                        (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.BaseAgility))
                        + Party.GetMemoryOffset(this._characterIndex);
                    GameMemory.Write(offset, byte.Parse(this.TextBaseAgility.Text), false);
                    break;
                case "TextBaseLuck":
                    offset =
                        (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.BaseLuck))
                        + Party.GetMemoryOffset(this._characterIndex);
                    GameMemory.Write(offset, byte.Parse(this.TextBaseLuck.Text), false);
                    break;
                case "TextBaseEvasion":
                    offset =
                        (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.BaseEvasion))
                        + Party.GetMemoryOffset(this._characterIndex);
                    GameMemory.Write(offset, byte.Parse(this.TextBaseEvasion.Text), false);
                    break;
                case "TextBaseAccuracy":
                    offset =
                        (int)Marshal.OffsetOf<PartyMember>(nameof(PartyMember.BaseAccuracy))
                        + Party.GetMemoryOffset(this._characterIndex);
                    GameMemory.Write(offset, byte.Parse(this.TextBaseAccuracy.Text), false);
                    break;
            }
            AeonsPanel.UpdateTabs();
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
}

public class AeonDefaults
{
    public string Name { get; }
    public int Hp { get; }
    public int Mp { get; }
    public byte Str { get; }
    public byte Def { get; }
    public byte Mag { get; }
    public byte MagDef { get; }
    public byte Agi { get; }
    public byte Luck { get; }
    public byte Eva { get; }
    public byte Acc { get; }
    public byte OdMax { get; }

    public AeonDefaults(
        string name,
        int hp,
        int mp,
        byte str,
        byte def,
        byte mag,
        byte magDef,
        byte agi,
        byte luck,
        byte eva,
        byte acc,
        byte odMax = 20
    )
    {
        Name = name;
        Hp = hp;
        Mp = mp;
        Str = str;
        Def = def;
        Mag = mag;
        MagDef = magDef;
        Agi = agi;
        Luck = luck;
        Eva = eva;
        Acc = acc;
        OdMax = odMax;
    }
}
