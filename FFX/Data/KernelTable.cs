using System;
using System.IO;
using System.Runtime.InteropServices;
using Farplane.Common;
using Farplane.Memory;

namespace Farplane.FFX.Data;

public class KernelTable
{
    static readonly int stringOffset1 = (int)
        Marshal.OffsetOf<KernelBlockHeader>(nameof(KernelBlockHeader.StringOffset1));
    static readonly int stringOffset2 = (int)
        Marshal.OffsetOf<KernelBlockHeader>(nameof(KernelBlockHeader.StringOffset2));

    readonly byte[] _stringTable;
    readonly byte[] _dataBlock;

    KernelFileHeader _kernelHeader;

    public int BlockCount => this._kernelHeader.DataLength / this._kernelHeader.BlockLength;
    public int BlockSize => this._kernelHeader.BlockLength;

    public KernelTable(int startOffset)
    {
        // Read kernel header
        this._kernelHeader = GameMemory.Read<KernelFileHeader>(startOffset, false);

        // Read block data
        this._dataBlock = GameMemory.Read<byte>(
            startOffset + 0x10,
            this._kernelHeader.DataLength,
            false
        );

        // Calculate string table length
        var stringTableLength = 0;

        // Find the last referenced string offset
        for (var i = 0; i < this.BlockCount; i++)
        {
            var blockOffset = i * this._kernelHeader.BlockLength;
            var string1 = BitConverter.ToInt16(this._dataBlock, blockOffset + stringOffset1);
            var string2 = BitConverter.ToInt16(this._dataBlock, blockOffset + stringOffset2);

            if (string1 > stringTableLength)
            {
                stringTableLength = string1;
            }

            if (string2 > stringTableLength)
            {
                stringTableLength = string2;
            }
        }

        // Add length of last string to table length
        var lastString = GameMemory.Read<byte>(
            startOffset + this._kernelHeader.DataLength + 0x14 + stringTableLength,
            512,
            false
        );
        var lastStringLength = Array.IndexOf(lastString, (byte)0) + 3; // for 0x004700 end string marker
        stringTableLength += lastStringLength;

        // Read string table
        this._stringTable = GameMemory.Read<byte>(
            startOffset + this._kernelHeader.DataLength + 0x14,
            stringTableLength,
            false
        );
    }

    public KernelTable(string fileName)
    {
        using (var bw = new BinaryReader(new FileStream(fileName, FileMode.Open)))
        {
            // Read kernel header
            // Read kernel header
            var headerBytes = new byte[0x10];
            bw.Read(headerBytes, 0, 0x10);
            this._kernelHeader = StructHelper.BytesToStruct<KernelFileHeader>(headerBytes);

            // Read block data
            this._dataBlock = bw.ReadBytes(this._kernelHeader.DataLength);

            // skip 4 bytes
            bw.BaseStream.Seek(4, SeekOrigin.Current);

            // Read string table
            this._stringTable = bw.ReadBytes((int)(bw.BaseStream.Length - bw.BaseStream.Position));
        }
    }

    public string GetString1(int blockIndex)
    {
        var block = this.GetBlock(blockIndex);
        var stringOffset = BitConverter.ToUInt16(
            this._dataBlock,
            (blockIndex * this._kernelHeader.BlockLength) + stringOffset1
        );
        var stringBytes = new byte[
            stringOffset + 512 > this._stringTable.Length
                ? this._stringTable.Length - stringOffset
                : 512
        ];
        Array.Copy(this._stringTable, stringOffset, stringBytes, 0, stringBytes.Length);
        return StringConverter.ToASCII(stringBytes);
    }

    public string GetString2(int blockIndex)
    {
        var block = this.GetBlock(blockIndex);
        var stringOffset = BitConverter.ToUInt16(
            this._dataBlock,
            (blockIndex * this._kernelHeader.BlockLength) + stringOffset2
        );
        var stringBytes = new byte[
            stringOffset + 512 > this._stringTable.Length
                ? this._stringTable.Length - stringOffset
                : 512
        ];
        Array.Copy(this._stringTable, stringOffset, stringBytes, 0, stringBytes.Length);
        return StringConverter.ToASCII(stringBytes);
    }

    public byte[] GetBlock(int blockIndex)
    {
        var kernelData = new byte[this._kernelHeader.BlockLength];
        Array.Copy(
            this._dataBlock,
            this._kernelHeader.BlockLength * blockIndex,
            kernelData,
            0,
            kernelData.Length
        );

        return kernelData;
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 0, Size = 16)]
public struct KernelFileHeader
{
    [MarshalAs(UnmanagedType.U4)]
    public int TableHeader;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
    public byte[] Padding;

    [MarshalAs(UnmanagedType.U1)]
    public byte LastBlockIndex;

    [MarshalAs(UnmanagedType.U1)]
    public byte unknown;

    [MarshalAs(UnmanagedType.U2)]
    public ushort BlockLength;

    [MarshalAs(UnmanagedType.U2)]
    public ushort DataLength;
}

[StructLayout(LayoutKind.Sequential, Pack = 0, Size = 14)]
public struct KernelBlockHeader
{
    public ushort BlockID;
    public ushort unknown1;
    public ushort StringOffset1;
    public ushort unknown3;
    public ushort unknown4;
    public ushort unknown5;
    public ushort StringOffset2;
}

[StructLayout(LayoutKind.Sequential, Pack = 0)]
public struct KernelBlock
{
    public KernelBlockHeader BlockHeader;

    [MarshalAs(UnmanagedType.ByValArray)]
    public byte[] BlockData;
}
