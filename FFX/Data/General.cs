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
    static int _offsetSaveData => OffsetScanner.GetOffset(GameOffset.FFX_SaveDataBase);
    static int _offsetMinigameTimer => OffsetScanner.GetOffset(GameOffset.FFX_MinigameTimerPtr);

    public static int CurrentGil
    {
        get => GameMemory.Read<int>(_offsetCurrentGil, false);
        set => GameMemory.Write(_offsetCurrentGil, value, false);
    }

    public static int TidusOverdrive
    {
        get => GameMemory.Read<int>(_offsetTidusOverdrive, false);
        set => GameMemory.Write(_offsetTidusOverdrive, value, false);
    }

    public static uint YojimboCompatibility
    {
        get => GameMemory.Read<uint>(_offsetSaveData + 0x3DA4, false);
        set => GameMemory.Write(_offsetSaveData + 0x3DA4, Math.Min(value, 255u), false);
    }

    public static bool RikkuAlBhedCostume
    {
        get => GameMemory.Read<byte>(_offsetSaveData + 0x00D1, false) != 0;
        set => GameMemory.Write(_offsetSaveData + 0x00D1, (byte)(value ? 1 : 0), false);
    }

    public static ushort StoryProgress
    {
        get => GameMemory.Read<ushort>(_offsetSaveData + 0x0BEC, false);
        set => GameMemory.Write(_offsetSaveData + 0x0BEC, value, false);
    }

    public static uint TotalBattles
    {
        get => GameMemory.Read<uint>(_offsetSaveData + 0x3D14, false);
        set => GameMemory.Write(_offsetSaveData + 0x3D14, value, false);
    }

    public static uint SuccessfulRikkuSteals
    {
        get => GameMemory.Read<uint>(_offsetSaveData + 0x3DB0, false);
        set => GameMemory.Write(_offsetSaveData + 0x3DB0, value, false);
    }

    public static uint BribeGilSpent
    {
        get => GameMemory.Read<uint>(_offsetSaveData + 0x3DB4, false);
        set => GameMemory.Write(_offsetSaveData + 0x3DB4, value, false);
    }
    // --- Luca Theater Spheres ---
    public static byte MusicSpheres
    {
        get => GameMemory.Read<byte>(_offsetSaveData + 0x034A, false);
        set => GameMemory.Write(_offsetSaveData + 0x034A, Math.Min(value, (byte)71), false);
    }
    public static byte MovieSpheres
    {
        get => GameMemory.Read<byte>(_offsetSaveData + 0x034B, false);
        set => GameMemory.Write(_offsetSaveData + 0x034B, Math.Min(value, (byte)50), false);
    }
    public static void UnlockAllMusicSpheres() => MusicSpheres = 71;
    public static void UnlockAllMovieSpheres() => MovieSpheres = 50;

    // --- Minigame: Lightning Dodging ---
    public static ushort LightningTotalAll
    {
        get => GameMemory.Read<ushort>(_offsetSaveData + 0x03FC, false);
        set => GameMemory.Write(_offsetSaveData + 0x03FC, value, false);
    }

    public static ushort LightningTotalDodged
    {
        get => GameMemory.Read<ushort>(_offsetSaveData + 0x03FE, false);
        set => GameMemory.Write(_offsetSaveData + 0x03FE, value, false);
    }

    public static ushort LightningRecordStreak
    {
        get => GameMemory.Read<ushort>(_offsetSaveData + 0x0400, false);
        set => GameMemory.Write(_offsetSaveData + 0x0400, value, false);
    }

    public static void Complete200LightningDodges()
    {
        LightningRecordStreak = 200;
        if (LightningTotalDodged < 200)
            LightningTotalDodged = 200;
        if (LightningTotalAll < 200)
            LightningTotalAll = 200;
    }

    // --- Minigame: Chocobo Race ---
    public static void WinChocoboRace()
    {
        var timerPtr = GameMemory.Read<int>(_offsetMinigameTimer, false);
        if (timerPtr == 0)
            return;

        // Reset running time to 0.0s (2 bytes at +0x1D8)
        GameMemory.Write(timerPtr + 0x1D8, (ushort)0, false);
        // Set 50 balloons collected and 0 birds hit (+0x1FB: BalloonsOwn, +0x1FD: HitsOwn)
        GameMemory.Write(timerPtr + 0x1FB, (byte)50, false);
        GameMemory.Write(timerPtr + 0x1FD, (byte)0, false);
    }

    public static void FreezeChocoboTimer()
    {
        var timerPtr = GameMemory.Read<int>(_offsetMinigameTimer, false);
        if (timerPtr == 0)
            return;

        GameMemory.Write(timerPtr + 0x1D8, (ushort)0, false);
    }

    // --- Minigame: Blitzball Quick Win ---
    public static void FastWinBlitzballMatch(bool wholeGame = true)
    {
        var oppScore = GameMemory.Read<byte>(_offsetSaveData + 0x163F, false);
        GameMemory.Write(_offsetSaveData + 0x163E, (byte)(oppScore + 1), false); // Own Score = Opponent + 1

        if (wholeGame)
        {
            GameMemory.Write(_offsetSaveData + 0x1640, (byte)1, false); // Phase = Round 2
        }

        var timerPtr = GameMemory.Read<int>(_offsetMinigameTimer, false);
        if (timerPtr != 0)
        {
            GameMemory.Write(timerPtr + 0x24C, (ushort)300, false); // 5 minutes (300 seconds) to end the match
        }
    }

    // --- Temple Seals (Anima Unlocker) ---
    public static byte TempleSeals
    {
        get => GameMemory.Read<byte>(_offsetSaveData + 0x0C5C, false);
        set => GameMemory.Write(_offsetSaveData + 0x0C5C, value, false);
    }

    public static bool GetTempleSeal(int sealIndex)
    {
        var seals = TempleSeals;
        return (seals & (1 << sealIndex)) != 0;
    }

    public static void SetTempleSeal(int sealIndex, bool unlocked)
    {
        var seals = TempleSeals;
        if (unlocked)
            seals |= (byte)(1 << sealIndex);
        else
            seals &= (byte)~(1 << sealIndex);

        TempleSeals = seals;
    }

    public static void UnlockAllTempleSeals()
    {
        // 7 temples: Besaid (0), Kilika (1), Djose (2), Macalania (3), Bevelle (4), Zanarkand (5), Baaj (6)
        TempleSeals = 0x7F;
    }

    // --- Party Affection ---
    public static uint GetAffection(int charIndex)
    {
        if (charIndex < 0 || charIndex >= 8)
            return 0;
        return GameMemory.Read<uint>(_offsetSaveData + 0x002C + (charIndex * 4), false);
    }

    public static void SetAffection(int charIndex, uint value)
    {
        if (charIndex < 0 || charIndex >= 8)
            return;
        GameMemory.Write(_offsetSaveData + 0x002C + (charIndex * 4), value, false);
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
