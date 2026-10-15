using Farplane.Memory;

namespace Farplane.FFX.Data;

public class MonsterArena
{
    static int _offsetMonstersCaptured => OffsetScanner.GetOffset(GameOffset.FFX_MonstersCaptured);

    public static byte[] GetCaptureCounts() =>
        GameMemory.Read<byte>(_offsetMonstersCaptured, 139, false) ?? new byte[139];

    public static void SetCaptureCount(int index, byte count) =>
        GameMemory.Write(_offsetMonstersCaptured + index, count, false);
}
