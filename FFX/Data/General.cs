using System;
using System.IO;
using System.Runtime.InteropServices;
using Farplane.Memory;

namespace Farplane.FFX.Data;

public class General
{
    const string DumpFolder = "dump";
    static int _offsetCurrentGil => OffsetScanner.GetOffset(GameOffset.FFX_CurrentGil);
    static int _offsetTidusOverdrive => OffsetScanner.GetOffset(GameOffset.FFX_TidusOverdrive);

    public static int CurrentGil
    {
        get => GameMemory.Read<int>(_offsetCurrentGil, false);
        set => GameMemory.Write(_offsetCurrentGil, value, false);
    }

    public static int TidusOverdrive
    {
        get => GameMemory.Read<int>(_offsetTidusOverdrive, false);
        set => GameMemory.Write(_offsetTidusOverdrive, value);
    }

    public static void DumpStruct<T>(T structData, string fileName)
    {
        var structLength = Marshal.SizeOf<T>();
        var structPtr = Marshal.AllocHGlobal(structLength);
        var structBytes = new byte[structLength];
        try
        {
            Marshal.StructureToPtr(structData, structPtr, false);
            Marshal.Copy(structPtr, structBytes, 0, structLength);
            if (!Directory.Exists(DumpFolder))
            {
                Directory.CreateDirectory(DumpFolder);
            }
            File.WriteAllBytes(Path.Combine(DumpFolder, fileName), structBytes);
        }
        finally
        {
            Marshal.FreeHGlobal(structPtr);
        }

        var structFile = Path.Combine(DumpFolder, $"struct_{typeof(T).Name}.txt");
        if (!File.Exists(structFile))
        {
            foreach (var field in typeof(T).GetFields())
            {
                File.AppendAllText(
                    structFile,
                    $"{field.Name} {Marshal.OffsetOf<T>(field.Name).ToString("X4")}{Environment.NewLine}"
                );
            }
        }
    }
}
