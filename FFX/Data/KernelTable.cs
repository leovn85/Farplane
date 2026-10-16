using System;
using System.IO;
using System.Runtime.InteropServices;
using Farplane.Common;
using Farplane.Memory;

namespace Farplane.FFX.Data;

public enum KernelType
{
    Command,
    Item
}

public class KernelTable
{
    public const int EntrySize = 0x60; // 96 bytes per command / item entry
    public const int HeaderSize = 0x14; // 20 bytes entry list header

    readonly KernelType _type;
    readonly int _bufferAddress;
    readonly ushort _allocatedBufferSize;
    readonly int _entryCount;
    readonly byte[] _stringTable;

    public int EntryCount => this._entryCount;
    public int BufferAddress => this._bufferAddress;
    public ushort BufferSize => this._allocatedBufferSize;

    public KernelTable(KernelType type)
    {
        this._type = type;

        var ptrOffset = type == KernelType.Command
            ? OffsetScanner.GetOffset(GameOffset.FFX_CommandBinPointer)
            : OffsetScanner.GetOffset(GameOffset.FFX_ItemBinPointer);

        var sizeOffset = type == KernelType.Command
            ? OffsetScanner.GetOffset(GameOffset.FFX_SizeCommandBin)
            : OffsetScanner.GetOffset(GameOffset.FFX_SizeItemBin);

        if (ptrOffset == 0 || sizeOffset == 0)
        {
            return;
        }

        this._bufferAddress = GameMemory.Read<int>(ptrOffset, false);
        this._allocatedBufferSize = GameMemory.Read<ushort>(sizeOffset, false);

        if (this._bufferAddress == 0 || this._allocatedBufferSize == 0)
        {
            return;
        }

        this._entryCount = type == KernelType.Command ? 320 : 112;

        var stringTableStart = this._bufferAddress + HeaderSize + (this._entryCount * EntrySize);
        var stringTableLength = this._allocatedBufferSize - (HeaderSize + (this._entryCount * EntrySize));

        if (stringTableLength > 0 && stringTableLength < 0x10000)
        {
            this._stringTable = GameMemory.Read<byte>(stringTableStart, stringTableLength, false) ?? [];
        }
        else
        {
            this._stringTable = [];
        }
    }

    public string GetName(int index)
    {
        if (index < 0 || index >= this._entryCount || this._bufferAddress == 0)
        {
            return string.Empty;
        }

        var entryOffset = this._bufferAddress + HeaderSize + (index * EntrySize);
        var nameOffset = GameMemory.Read<ushort>(entryOffset, false);

        return this.ReadStringFromTable(nameOffset);
    }

    public string GetDescription(int index)
    {
        if (index < 0 || index >= this._entryCount || this._bufferAddress == 0)
        {
            return string.Empty;
        }

        var entryOffset = this._bufferAddress + HeaderSize + (index * EntrySize);
        var descOffset = GameMemory.Read<ushort>(entryOffset + 8, false);

        return this.ReadStringFromTable(descOffset);
    }

    string ReadStringFromTable(ushort stringOffset)
    {
        if (this._stringTable == null || stringOffset >= this._stringTable.Length)
        {
            return string.Empty;
        }

        var end = stringOffset;
        while (end < this._stringTable.Length && this._stringTable[end] != 0)
        {
            end++;
        }

        var len = end - stringOffset;
        var bytes = new byte[len];
        Array.Copy(this._stringTable, stringOffset, bytes, 0, len);

        return StringConverter.ToASCII(bytes);
    }

    public KernelCommandData ReadCommandData(int index)
    {
        if (index < 0 || index >= this._entryCount || this._bufferAddress == 0)
        {
            return default;
        }

        var dataOffset = this._bufferAddress + HeaderSize + (index * EntrySize) + 0x10;
        var bytes = GameMemory.Read<byte>(dataOffset, Marshal.SizeOf<KernelCommandData>(), false);

        return StructHelper.BytesToStruct<KernelCommandData>(bytes);
    }

    public void WriteCommandData(int index, KernelCommandData data)
    {
        if (index < 0 || index >= this._entryCount || this._bufferAddress == 0)
        {
            return;
        }

        var dataOffset = this._bufferAddress + HeaderSize + (index * EntrySize) + 0x10;
        var bytes = StructHelper.StructToBytes(data);

        GameMemory.Write(dataOffset, bytes, false);
    }

    public byte[] ExportBuffer()
    {
        if (this._bufferAddress == 0 || this._allocatedBufferSize == 0)
        {
            return [];
        }

        return GameMemory.Read<byte>(this._bufferAddress, this._allocatedBufferSize, false) ?? [];
    }

    public bool HotReload(byte[] fileBytes, out string error)
    {
        error = string.Empty;
        if (this._bufferAddress == 0 || this._allocatedBufferSize == 0)
        {
            error = "Game kernel buffer is not currently loaded in memory.";
            return false;
        }

        if (fileBytes == null || fileBytes.Length == 0)
        {
            error = "Selected file is empty.";
            return false;
        }

        if (fileBytes.Length > this._allocatedBufferSize)
        {
            error = $"File size ({fileBytes.Length:N0} bytes) exceeds game's allocated buffer capacity ({this._allocatedBufferSize:N0} bytes)!";
            return false;
        }

        GameMemory.Write(this._bufferAddress, fileBytes, false);
        return true;
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct KernelCommandData
{
    public short Anim1Id;
    public short Anim2Id;
    public byte IconId;
    public byte CasterAnimId;
    public byte MenuFlags;
    public byte SubSubMenu;
    public byte SubMenu;
    public sbyte CharacterUser;
    public byte TargetFlags;
    public byte TargetsAllowed;
    public byte Misc1Flags;
    public byte Misc2Flags;
    public byte Misc3Flags;
    public byte Misc4Flags;
    public byte DamageFlags;
    public byte StealGil;
    public byte PreviewFlags;
    public byte DamageTypeFlags;
    public byte MoveRank;
    public byte CostMp;
    public byte CostOverdrive;
    public byte AttackCritBonus;
    public byte DamageFormula;
    public byte AttackAccuracy;
    public byte AttackPower;
    public byte HitCount;
    public byte ShatterChance;
    public byte ElementFlags;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 25)]
    public byte[] StatusChance;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 13)]
    public byte[] StatusDuration;

    public ushort StatusFlags;
    public ushort StatBuffFlags;
    public byte OverdriveCategory;
    public byte StatBuffValue;
    public ushort SpecialBuffFlags;

    public byte OrderingIndexInMenu;
    public sbyte SphereTypeForSphereGrid;
    public byte Unk1;
    public byte Unk2;
}
