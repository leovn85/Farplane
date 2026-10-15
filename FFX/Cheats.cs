using System.Runtime.InteropServices;
using Farplane.Common;
using Farplane.FFX.Data;
using Farplane.Memory;

namespace Farplane.FFX;

static class Cheats
{
    public static void GiveAllItems()
    {
        for (var i = 1; i < Item.Items.Length - 1; i++)
        {
            Item.WriteItem(i - 1, Item.Items[i].ID, 99);
        }
    }

    public static void MaxAllStats()
    {
        var partyOffset = OffsetScanner.GetOffset(GameOffset.FFX_PartyStatBase);

        for (var i = 0; i < 18; i++)
        {
            var characterOffset = partyOffset + (0x94 * i);
            var overdriveLevel = i > 7 ? 20 : 100;

            Party.SetPartyMemberAttribute<uint>(i, nameof(PartyMember.CurrentHp), 99999);
            Party.SetPartyMemberAttribute<uint>(i, nameof(PartyMember.CurrentHpMax), 99999);
            Party.SetPartyMemberAttribute<uint>(i, nameof(PartyMember.BaseHp), 99999);
            Party.SetPartyMemberAttribute<uint>(i, nameof(PartyMember.CurrentMp), 9999);
            Party.SetPartyMemberAttribute<uint>(i, nameof(PartyMember.CurrentMpMax), 9999);
            Party.SetPartyMemberAttribute<uint>(i, nameof(PartyMember.BaseMp), 9999);

            Party.SetPartyMemberAttribute<byte>(i, nameof(PartyMember.BaseStrength), 255);
            Party.SetPartyMemberAttribute<byte>(i, nameof(PartyMember.BaseDefense), 255);
            Party.SetPartyMemberAttribute<byte>(i, nameof(PartyMember.BaseMagic), 255);
            Party.SetPartyMemberAttribute<byte>(i, nameof(PartyMember.BaseMagicDefense), 255);
            Party.SetPartyMemberAttribute<byte>(i, nameof(PartyMember.BaseAgility), 255);
            Party.SetPartyMemberAttribute<byte>(i, nameof(PartyMember.BaseLuck), 255);
            Party.SetPartyMemberAttribute<byte>(i, nameof(PartyMember.BaseEvasion), 255);
            Party.SetPartyMemberAttribute<byte>(i, nameof(PartyMember.BaseAccuracy), 255);
            Party.SetPartyMemberAttribute(
                i,
                nameof(PartyMember.OverdriveLevel),
                (byte)overdriveLevel
            );
            Party.SetPartyMemberAttribute(
                i,
                nameof(PartyMember.OverdriveMax),
                (byte)overdriveLevel
            );
        }
    }

    public static void MaxSphereLevels()
    {
        var partyOffset = OffsetScanner.GetOffset(GameOffset.FFX_PartyStatBase);
        for (var i = 0; i < 8; i++)
        {
            var characterOffset = partyOffset + (0x94 * i);
            LegacyMemoryReader.WriteByte(
                StructHelper.GetFieldOffset<PartyMember>(
                    nameof(PartyMember.SphereLevelCurrent),
                    characterOffset
                ),
                255
            );
        }
    }

    public static void LearnAllAbilities()
    {
        var partyOffset = OffsetScanner.GetOffset(GameOffset.FFX_PartyStatBase);
        for (var i = 0; i < 18; i++)
        {
            var characterAbilityOffset =
                partyOffset
                + (Marshal.SizeOf<PartyMember>() * i)
                + StructHelper.GetFieldOffset<PartyMember>(nameof(PartyMember.SkillFlags));

            var currentAbilities = LegacyMemoryReader.ReadBytes(characterAbilityOffset, 13);

            // Flip all normal ability bits
            currentAbilities[1] |= 0xF0;
            for (var b = 2; b < 11; b++)
            {
                currentAbilities[b] |= 0xFF;
            }

            currentAbilities[11] |= 0x0F;
            currentAbilities[12] |= 0xFF;

            LegacyMemoryReader.WriteBytes(characterAbilityOffset, currentAbilities);
        }
    }
}
