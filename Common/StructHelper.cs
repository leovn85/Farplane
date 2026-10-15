using System;
using System.Runtime.InteropServices;

namespace Farplane.Common;

public static class StructHelper
{
    public static int GetSize<T>() where T : struct => Marshal.SizeOf<T>();

    public static int GetFieldOffset<T>(string fieldName, int baseOffset = 0) =>
        baseOffset + (int)Marshal.OffsetOf<T>(fieldName);

    public static T BytesToStruct<T>(byte[] bytes, int startIndex = 0) where T : struct
    {
        var size = Marshal.SizeOf<T>();
        if (bytes == null || bytes.Length < startIndex + size)
        {
            return default;
        }

        var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try
        {
            var ptr = IntPtr.Add(handle.AddrOfPinnedObject(), startIndex);
            return Marshal.PtrToStructure<T>(ptr);
        }
        finally
        {
            handle.Free();
        }
    }

    public static byte[] StructToBytes<T>(T structure) where T : struct
    {
        var size = Marshal.SizeOf<T>();
        var bytes = new byte[size];
        var ptr = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(structure, ptr, false);
            Marshal.Copy(ptr, bytes, 0, size);
            return bytes;
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }
}
