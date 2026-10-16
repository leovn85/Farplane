using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Farplane.Common;
using Farplane.Memory;

namespace Farplane.FFX.Data;

public static class Equipment
{
    public static int Offset => OffsetScanner.GetOffset(GameOffset.FFX_EquipmentBase);
    public const int BlockLength = 0x16;
    public const int MaxItems = 0xB2; // 178 equipment slots (0x00 to 0xB1)

    // Base prices for Auto-Abilities 0..128 (verified from FFX arms_rate.bin)
    private static readonly int[] AutoAbilityBasePrices =
    [
        200, 6000, 6000, 4000, 22000, 25000, 17000, 8000, 12000, 10000,
        20000, 0, 20000, 30000, 30000, 40000, 20000, 25000, 40000, 60000,
        0, 40000, 60000, 0, 0, 0, 0, 6000, 6000, 6500,
        700, 1500, 3000, 10000, 700, 1500, 3000, 10000, 700, 1500,
        3000, 10000, 700, 1500, 3000, 10000, 20000, 8000, 4000, 2000,
        12000, 8000, 4000, 2000, 12000, 5000, 2500, 1200, 10000, 1500,
        1500, 400, 8000, 1000, 800, 200, 8000, 3000, 2000, 500,
        4000, 2000, 2000, 500, 7000, 3000, 3000, 1000, 4000, 1000,
        4000, 1000, 8000, 4000, 22000, 22000, 50000, 60000, 14000, 4000,
        4000, 20000, 20000, 3000, 500, 500, 500, 500, 100, 500,
        5000, 10000, 100, 500, 5000, 10000, 200, 500, 5000, 10000,
        200, 500, 5000, 10000, 100, 1000, 6000, 15000, 1200, 6000,
        8000, 18000, 6000, 0, 10, 10, 10, 10, 50000
    ];

    // Ability indices deemed rare/valuable to protect against accidental bulk sale
    public static readonly HashSet<int> RareAbilityIndices =
    [
        10,  // Auto-Phoenix
        13,  // One MP Cost
        14,  // Double Overdrive
        15,  // Triple Overdrive
        18,  // Double AP
        19,  // Triple AP
        22,  // Master Thief
        23,  // Break HP Limit
        24,  // Break MP Limit
        25,  // Break Damage Limit
        26,  // Gillionaire
        86,  // Auto-Haste
        128  // Ribbon
    ];

    public static EquipmentItem ReadItem(int itemIndex)
    {
        var offset = Offset + (itemIndex * BlockLength);
        var bytes = GameMemory.Read<byte>(offset, BlockLength, false);
        return StructHelper.BytesToStruct<EquipmentItem>(bytes);
    }

    public static void WriteItem(int itemIndex, EquipmentItem item)
    {
        var itemBytes = StructHelper.StructToBytes(item);
        var offset = Offset + (itemIndex * BlockLength);
        GameMemory.Write(offset, itemBytes, false);
    }

    public static EquipmentItem[] ReadItems()
    {
        var dataLength = MaxItems * BlockLength;
        var dataBytes = GameMemory.Read<byte>(Offset, dataLength, false);
        var readItems = new EquipmentItem[MaxItems];
        if (dataBytes == null || dataBytes.Length < dataLength)
        {
            return readItems;
        }
        for (var i = 0; i < MaxItems; i++)
        {
            readItems[i] = StructHelper.BytesToStruct<EquipmentItem>(dataBytes, i * BlockLength);
        }
        return readItems;
    }

    public static int GetSellRate(ushort rawAbilityId)
    {
        if (rawAbilityId is 0 or 0xFF or 0xFFFF)
        {
            return 0;
        }

        var abilityIndex = rawAbilityId & 0x0FFF;
        if (abilityIndex >= AutoAbilityBasePrices.Length)
        {
            return 0;
        }

        return AutoAbilityBasePrices[abilityIndex] / 4;
    }

    public static int CalculateSellPrice(EquipmentItem item)
    {
        if (item.SlotOccupied == 0)
        {
            return 0;
        }

        var validAbilityCount = 0;
        var sumRates = 0;
        var abilities = item.Abilities ?? new ushort[4];

        for (var i = 0; i < item.AbilityCount && i < abilities.Length; i++)
        {
            var raw = abilities[i];
            if (raw is not 0 and not 0xFF and not 0xFFFF)
            {
                var idx = raw & 0x0FFF;
                if (idx < AutoAbilityBasePrices.Length)
                {
                    validAbilityCount++;
                    sumRates += AutoAbilityBasePrices[idx] / 4;
                }
            }
        }

        int price;
        switch (item.AbilityCount)
        {
            case 0:
                price = 6;
                break;
            case 1:
                price = sumRates + 12;
                break;
            case 2:
                price = (int)(sumRates * 1.5) + 18;
                break;
            case 3:
                price = (int)(sumRates * 4.5) + 56;
                if (validAbilityCount > 1)
                {
                    price = (int)(price / 1.5);
                }
                break;
            case 4:
                price = (sumRates * 15) + 187;
                if (validAbilityCount == 2)
                {
                    price /= 2;
                }
                else if (validAbilityCount > 2)
                {
                    price /= 3;
                }
                break;
            default:
                price = 6;
                break;
        }

        return Math.Max(price, 6);
    }

    public static HashSet<int> GetEquippedSlots()
    {
        var equippedSlots = new HashSet<int>();
        for (var i = 0; i < 18; i++)
        {
            var member = Party.ReadPartyMember(i);
            if (member.EquippedWeapon < MaxItems)
            {
                equippedSlots.Add(member.EquippedWeapon);
            }
            if (member.EquippedArmor < MaxItems)
            {
                equippedSlots.Add(member.EquippedArmor);
            }
        }
        return equippedSlots;
    }

    public static bool HasRareAbilities(EquipmentItem item)
    {
        var abilities = item.Abilities ?? new ushort[4];
        for (var i = 0; i < item.AbilityCount && i < abilities.Length; i++)
        {
            var raw = abilities[i];
            if (raw is not 0 and not 0xFF and not 0xFFFF)
            {
                var index = raw & 0x0FFF;
                if (RareAbilityIndices.Contains(index))
                {
                    return true;
                }
            }
        }
        return false;
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 0, CharSet = CharSet.Ansi)]
public struct EquipmentItem
{
    [MarshalAs(UnmanagedType.U1)]
    public byte Name;
    [MarshalAs(UnmanagedType.U1)]
    public byte Name2;
    [MarshalAs(UnmanagedType.U1)]
    public byte SlotOccupied;
    [MarshalAs(UnmanagedType.U1)]
    public byte Flags; // 0x01: Summon, 0x02: Hidden, 0x04: Celestial, 0x08: Brotherhood
    [MarshalAs(UnmanagedType.U1)]
    public byte Character;
    [MarshalAs(UnmanagedType.U1)]
    public byte Type;
    [MarshalAs(UnmanagedType.U2)]
    public ushort EquippedBy;
    [MarshalAs(UnmanagedType.U1)]
    public byte DamageFormula;
    [MarshalAs(UnmanagedType.U1)]
    public byte AttackPower;
    [MarshalAs(UnmanagedType.U1)]
    public byte Critical;
    [MarshalAs(UnmanagedType.U1)]
    public byte AbilityCount;
    [MarshalAs(UnmanagedType.U2)]
    public ushort Appearance;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.U2)]
    public ushort[] Abilities;
}
