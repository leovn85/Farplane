using System.Runtime.InteropServices;
using Farplane.Common;
using Farplane.Memory;

namespace Farplane.FFX.Data;

public static class Equipment
{
    public static int Offset => OffsetScanner.GetOffset(GameOffset.FFX_EquipmentBase);
    public const int BlockLength = 0x16;
    public const int MaxItems = 0xB2;

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
    public byte CustomizeMode;
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
