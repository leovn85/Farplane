using Farplane.Memory;

namespace Farplane.FFX.Data;

public class MonsterArena
{
    static int _offsetMonstersCaptured => OffsetScanner.GetOffset(GameOffset.FFX_MonstersCaptured);

    public static byte[] GetCaptureCounts() =>
        GameMemory.Read<byte>(_offsetMonstersCaptured, 139, false) ?? new byte[139];

    public static void SetCaptureCount(int index, byte count) =>
        GameMemory.Write(_offsetMonstersCaptured + index, count, false);

    public static bool IsCreationUnlocked(int index)
    {
        if (index < 104 || index > 138)
        {
            return false;
        }
        var val = GameMemory.Read<byte>(_offsetMonstersCaptured + index, false);
        return val != 0;
    }

    public static void SetCreationUnlocked(int index, bool unlocked)
    {
        if (index < 104 || index > 138)
        {
            return;
        }
        GameMemory.Write(_offsetMonstersCaptured + index, (byte)(unlocked ? 1 : 0), false);
    }

    public static void UnlockAllCreations()
    {
        for (var i = 104; i <= 138; i++)
        {
            GameMemory.Write(_offsetMonstersCaptured + i, (byte)1, false);
        }
    }

    public static void LockAllCreations()
    {
        for (var i = 104; i <= 138; i++)
        {
            GameMemory.Write(_offsetMonstersCaptured + i, (byte)0, false);
        }
    }

    public static void CaptureAllMonsters(byte count = 10)
    {
        for (var i = 0; i < 104; i++)
        {
            GameMemory.Write(_offsetMonstersCaptured + i, count, false);
        }
    }

    public static void ReleaseAllMonsters()
    {
        for (var i = 0; i < 104; i++)
        {
            GameMemory.Write(_offsetMonstersCaptured + i, (byte)0, false);
        }
    }
}
