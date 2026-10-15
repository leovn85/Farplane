using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Farplane.Memory;

public static class OffsetScanner
{
    static readonly SigScan _sigScan = new();

    static readonly OffsetBytePattern[] _bytePatterns =
    [
        new OffsetBytePattern()
        {
            Type = GameOffset.FFX_ItemTypes,
            Pattern =
                "55 8B EC 8B 45 08 85 C0 74 06 C7 00 70 00 00 00 B8 ?? ?? ?? ?? 5D C3"
                + "?? ?? ?? ?? ?? ?? ?? ?? ??"
                + "55 8B EC 8B 45 08 85 C0 74 06 C7 00 70 00 00 00 B8 ?? ?? ?? ?? 5D C3",
            ByteOffset = 49,
        },
        new OffsetBytePattern()
        {
            Type = GameOffset.FFX_ItemCounts,
            Pattern =
                "55 8B EC 8B 45 08 85 C0 74 06 C7 00 70 00 00 00 B8 ?? ?? ?? ?? 5D C3"
                + "?? ?? ?? ?? ?? ?? ?? ?? ??"
                + "55 8B EC 8B 45 08 85 C0 74 06 C7 00 70 00 00 00 B8 ?? ?? ?? ?? 5D C3",
            ByteOffset = 17,
        },
        new OffsetBytePattern()
        {
            Type = GameOffset.FFX_PartyStatBase,
            Pattern = "81 C6 ?? ?? ?? ?? 83 7D 0C 00",
            ByteOffset = 2,
        },
        new OffsetBytePattern()
        {
            Type = GameOffset.FFX_EquipmentBase,
            Pattern = "6B F6 16 81 C6 ?? ?? ?? ?? EB 2A 85 F6",
            ByteOffset = 5,
        },
        new OffsetBytePattern()
        {
            Type = GameOffset.FFX_KeyItems,
            Pattern = "C1 F8 04 BE 01 00 00 00 D3 E6 0F B7 0C 45 ?? ?? ?? ?? 85 F1 5E",
            ByteOffset = 14,
        },
        new OffsetBytePattern()
        {
            Type = GameOffset.FFX_AlBhed,
            Pattern = "51 a1 ?? ?? ?? ?? 53 8b 1d ?? ?? ?? ?? 56 57 33 ff",
            ByteOffset = 9,
        },
        new OffsetBytePattern()
        {
            Type = GameOffset.FFX_SphereGrid,
            Pattern = "d9 54 24 14 66 a3 ?? ?? ?? ?? d9 05 ?? ?? ?? ?? 33 c0",
            ByteOffset = 6,
        },
        new OffsetBytePattern()
        {
            Type = GameOffset.FFX_CurrentGil,
            Pattern = "c3 c7 05 ?? ?? ?? ?? ff c9 9a 3b 3d ff c9 9a 3b",
            ByteOffset = 3,
        },
        new OffsetBytePattern()
        {
            Type = GameOffset.FFX_TidusOverdrive,
            Pattern =
                "50 e8 ?? ?? ?? ?? 83 c4 04 ff 05 ?? ?? ?? ?? BF ?? ?? ?? ?? BE ?? ?? ?? ?? 0F B7 1E",
            ByteOffset = 11,
        },
        new OffsetBytePattern()
        {
            Type = GameOffset.FFX_PartyList,
            Pattern = "8b 45 08 85 c0 74 06 c7 00 03 00 00 00 b8 ?? ?? ?? ?? 5d c3",
            ByteOffset = 14,
        },
        new OffsetBytePattern()
        {
            Type = GameOffset.FFX_MonstersCaptured,
            Pattern = "8b 45 08 25 ff 0f 00 00 0f b6 80 ?? ?? ?? ?? 5d c3",
            ByteOffset = 11,
        },
        new OffsetBytePattern()
        {
            Type = GameOffset.FFX_AeonNames,
            Pattern = "8d 04 b6 5e 8d 04 85 ?? ?? ?? ?? 5d c3",
            ByteOffset = 7,
        },
        new OffsetBytePattern()
        {
            Type = GameOffset.FFX_InBattleFlags,
            Pattern = "74 18 6a 01 c6 81 ?? ?? ?? ?? 01 ff 70 04 51 51",
            ByteOffset = 6,
        },
        new OffsetBytePattern()
        {
            Type = GameOffset.FFX_GainedAPFlags,
            Pattern = "74 0c 83 fb 08 7d 07 c6 83 ?? ?? ?? ?? 01 0f be 87",
            ByteOffset = 9,
        },
        new OffsetBytePattern()
        {
            Type = GameOffset.FFX_BattlePointerEnemy,
            Pattern = "0f b6 45 08 69 c0 90 0f 00 00 03 05 ?? ?? ?? ?? 5d c3",
            ByteOffset = 12,
        },
        new OffsetBytePattern()
        {
            Type = GameOffset.FFX_BattlePointerParty,
            Pattern = "83 f8 1f 7d 0e 69 c0 90 0f 00 00 03 05 ?? ?? ?? ?? 5d c3",
            ByteOffset = 13,
        },
        new OffsetBytePattern()
        {
            Type = GameOffset.FFX_CurrentRoom,
            Pattern = "0f b7 05 ?? ?? ?? ?? 3b 05 ?? ?? ?? ?? 0f 85 d3 00 00 00",
            ByteOffset = 9,
        },
        new OffsetBytePattern()
        {
            Type = GameOffset.FFX_DebugFlags,
            Pattern = "83 f8 01 75 09 80 3d ?? ?? ?? ?? 00 75 17 ff 75 08",
            ByteOffset = 7,
        },
        new OffsetBytePattern()
        {
            Type = GameOffset.FFX_BlitzballData,
            Pattern = "6a 05 b9 78 00 00 00 bf ?? ?? ?? ?? 68 ?? ?? ?? ?? f3 ab",
            ByteOffset = 8,
        },
        new OffsetBytePattern()
        {
            Type = GameOffset.FFX_BlitzballPointer,
            Pattern = "55 8b ec 8b 45 08 a3 ?? ?? ?? ?? 5d c3",
            ByteOffset = 7,
        },
        new OffsetBytePattern()
        {
            Type = GameOffset.FFX_BlitzballMode,
            Pattern = "55 8b ec a1 ?? ?? ?? ?? 3b 45 08 75 0a c7 05 ?? ?? ?? ?? FF FF FF FF",
            ByteOffset = 4,
        },
        new OffsetBytePattern()
        {
            Type = GameOffset.FFX_BlitzballTeamData,
            Pattern =
                "0F 87 B2 00 00 00 FF 24 85 ?? ?? ?? ?? A1 ?? ?? ?? ?? 8B F2 81 E6 FF FF FF F0",
            ByteOffset = 14,
        },
        new OffsetBytePattern()
        {
            Type = GameOffset.FFX_SkillTablePointer,
            Pattern =
                "55 8b ec ff 75 0c 8b 45 08 ff 35 ?? ?? ?? ?? 25 ff 0f 00 00 50 e8 96 ad 01 00 83 c4 0c 5d c3",
            ByteOffset = 11,
        },
    ];

    static readonly object _lock = new object();
    static readonly Dictionary<GameOffset, int> _offsets = new Dictionary<GameOffset, int>();
    static readonly HashSet<GameOffset> _failedOffsets = new HashSet<GameOffset>();

    public static int GetOffset(GameOffset type)
    {
        lock (_lock)
        {
            if (_failedOffsets.Contains(type))
            {
                return 0;
            }
            if (!_offsets.TryGetValue(type, out var foundOffset))
            {
                var searchSuccess = FindOffset(type);
                if (!searchSuccess)
                {
                    _failedOffsets.Add(type);
                    Console.WriteLine($"[OffsetScanner] Failed to locate offset: {type}");

                    Application.Current?.Dispatcher?.BeginInvoke(new Action(() =>
                    {
                        MessageBox.Show(
                            $"Unable to locate offset: {type}\n\n"
                                + "This feature may not work with the current game version. "
                                + "Other features will continue to work normally.",
                            "Offset not found",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );
                    }));
                    return 0;
                }
                foundOffset = _offsets[type];
            }
            return foundOffset;
        }
    }

    public static bool IsOffsetAvailable(GameOffset type)
    {
        if (_failedOffsets.Contains(type))
        {
            return false;
        }

        if (_offsets.ContainsKey(type))
        {
            return true;
        }

        return FindOffset(type);
    }

    public static void Reset()
    {
        lock (_lock)
        {
            _offsets.Clear();
            _failedOffsets.Clear();
            _sigScan.ResetRegion();
        }
    }

    static bool FindOffset(GameOffset offsetType)
    {
        // Build a table of offsets from the current process
        _sigScan.Process = GameMemory.Process;
        _sigScan.Address = GameMemory.Process.MainModule.BaseAddress;
        _sigScan.Size = GameMemory.Process.MainModule.ModuleMemorySize;

        var patternData = _bytePatterns.First(pattern => pattern.Type == offsetType);

        var pointer = _sigScan.FindPattern(patternData.Pattern, patternData.ByteOffset);
        if (pointer == IntPtr.Zero)
        {
            return false;
        }

        var offset = GameMemory.Read<IntPtr>((int)pointer, false);
        _offsets.Add(patternData.Type, (int)offset);

        return true;
    }
}

public class OffsetBytePattern
{
    public GameOffset Type { get; set; }
    public string Pattern { get; set; }
    public int ByteOffset { get; set; }
}

public enum GameOffset
{
    FFX_ItemTypes,
    FFX_ItemCounts,
    FFX_PartyStatBase,
    FFX_EquipmentBase,
    FFX_KeyItems,
    FFX_AlBhed,
    FFX_SphereGrid,
    FFX_CurrentGil,
    FFX_TidusOverdrive,
    FFX_PartyList,
    FFX_MonstersCaptured,
    FFX_AeonNames,
    FFX_InBattleFlags,
    FFX_GainedAPFlags,
    FFX_BattlePointerEnemy,
    FFX_BattlePointerParty,
    FFX_CurrentRoom,
    FFX_DebugFlags,
    FFX_BlitzballData,
    FFX_BlitzballPointer,
    FFX_BlitzballMode,
    FFX_BlitzballTeamData,
    FFX_SkillTablePointer,
    FFX_EnemySkillTablePointer,
}
