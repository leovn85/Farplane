using System;
using System.Diagnostics;
using Farplane.Memory;

namespace Farplane;

public static class LegacyMemoryReader
{
    static IntPtr _memoryHandle = IntPtr.Zero;
    static Process _memoryProcess;

    public static bool IsAttached => GameMemory.IsAttached;

    internal static void Attach(Process process)
    {
        Detach();
        if (process == null || process.HasExited)
        {
            return;
        }

        _memoryProcess = process;
        // Do not open an independent second handle if GameMemory is already open;
        // if GameMemory is not attached, open a handle with release management.
        if (!GameMemory.IsAttached)
        {
            _memoryHandle = WinAPI.OpenProcess(
                ProcessAccessFlags.VirtualMemoryOperation
                    | ProcessAccessFlags.VirtualMemoryRead
                    | ProcessAccessFlags.VirtualMemoryWrite
                    | ProcessAccessFlags.QueryInformation,
                false,
                process.Id
            );
        }
    }

    internal static bool CheckProcess()
    {
        if (GameMemory.IsAttached)
        {
            return true;
        }

        if (_memoryProcess == null || _memoryProcess.HasExited)
        {
            Detach();
            return false;
        }
        return true;
    }

    internal static void Detach()
    {
        if (_memoryHandle != IntPtr.Zero)
        {
            WinAPI.CloseHandle(_memoryHandle);
            _memoryHandle = IntPtr.Zero;
        }

        _memoryProcess?.Dispose();
        _memoryProcess = null;
    }

    public static int ReadInt32(int address, bool pointer = false)
    {
        if (GameMemory.IsAttached)
        {
            return GameMemory.Read<int>(address, isRelative: !pointer);
        }

        var bytes = ReadBytes(address, 4, pointer);
        return bytes != null && bytes.Length >= 4 ? BitConverter.ToInt32(bytes, 0) : 0;
    }

    public static short ReadInt16(int address, bool pointer = false)
    {
        if (GameMemory.IsAttached)
        {
            return GameMemory.Read<short>(address, isRelative: !pointer);
        }

        var bytes = ReadBytes(address, 2, pointer);
        return bytes != null && bytes.Length >= 2 ? BitConverter.ToInt16(bytes, 0) : (short)0;
    }

    public static uint ReadUInt32(int address, bool pointer = false)
    {
        if (GameMemory.IsAttached)
        {
            return GameMemory.Read<uint>(address, isRelative: !pointer);
        }

        var bytes = ReadBytes(address, 4, pointer);
        return bytes != null && bytes.Length >= 4 ? BitConverter.ToUInt32(bytes, 0) : 0U;
    }

    public static ushort ReadUInt16(int address, bool pointer = false)
    {
        if (GameMemory.IsAttached)
        {
            return GameMemory.Read<ushort>(address, isRelative: !pointer);
        }

        var bytes = ReadBytes(address, 2, pointer);
        return bytes != null && bytes.Length >= 2 ? BitConverter.ToUInt16(bytes, 0) : (ushort)0;
    }

    public static bool ReadByteFlag(int address, bool pointer = false)
    {
        if (GameMemory.IsAttached)
        {
            return GameMemory.Read<byte>(address, isRelative: !pointer) == 1;
        }

        var bytes = ReadBytes(address, 1, pointer);
        return bytes != null && bytes.Length > 0 && bytes[0] == 1;
    }

    public static byte ReadByte(int address, bool pointer = false)
    {
        if (GameMemory.IsAttached)
        {
            return GameMemory.Read<byte>(address, isRelative: !pointer);
        }

        var bytes = ReadBytes(address, 1, pointer);
        return bytes != null && bytes.Length > 0 ? bytes[0] : (byte)0;
    }

    public static byte[] ReadBytes(int address, int length, bool pointer = false)
    {
        if (GameMemory.IsAttached)
        {
            return GameMemory.Read<byte>(address, length, isRelative: !pointer);
        }

        if (_memoryHandle == IntPtr.Zero || _memoryProcess == null || _memoryProcess.HasExited)
        {
            return null;
        }

        var readBuffer = new byte[length];
        var baseAddr = pointer ? IntPtr.Zero : _memoryProcess.MainModule.BaseAddress;
        var success = WinAPI.ReadProcessMemory(
            _memoryHandle,
            baseAddr + address,
            readBuffer,
            length,
            out _
        );

        return success ? readBuffer : null;
    }

    public static bool WriteBytes(int address, byte[] bytes, bool pointer = false)
    {
        if (bytes == null || bytes.Length == 0)
        {
            return false;
        }

        if (GameMemory.IsAttached)
        {
            GameMemory.Write(address, bytes, isRelative: !pointer);
            return true;
        }

        if (_memoryHandle == IntPtr.Zero || _memoryProcess == null || _memoryProcess.HasExited)
        {
            return false;
        }

        var baseAddr = pointer ? IntPtr.Zero : _memoryProcess.MainModule.BaseAddress;
        return WinAPI.WriteProcessMemory(
            _memoryHandle,
            baseAddr + address,
            bytes,
            bytes.Length,
            out _
        );
    }

    public static bool WriteByte(int address, byte b, bool pointer = false)
    {
        if (GameMemory.IsAttached)
        {
            GameMemory.Write(address, b, isRelative: !pointer);
            return true;
        }

        return WriteBytes(address, [b], pointer);
    }
}
