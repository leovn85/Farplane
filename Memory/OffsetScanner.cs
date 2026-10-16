using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Farplane.Memory;

public static class OffsetScanner
{
    static readonly SigScan _sigScan = new();

    // AOBs <=> IDA Pro
    static readonly OffsetBytePattern[] _bytePatterns =
    [
        // Anchor 1: FFX_SaveDataBase (push offset word_D2CA90, Total Size: 0x68C0)
        new OffsetBytePattern
        {
            Type = GameOffset.FFX_SaveDataBase,
            Pattern = "68 ?? ?? ?? ?? E8 ?? ?? ?? ?? E8 ?? ?? ?? ?? 8B 40 10",
            ByteOffset = 1,
            FallbackRva = 0xD2CA90,
            IsDirectAddress = false
        },

        // Anchor 2: FFX_BtlBase (push offset dword_D2A8D0, Total Size: 0x2150)
        new OffsetBytePattern
        {
            Type = GameOffset.FFX_BtlBase,
            Pattern = "68 ?? ?? ?? ?? C7 05 ?? ?? ?? ?? ?? ?? ?? ?? E8 ?? ?? ?? ?? 83 C4 08 E9 0F CA 01 00",
            ByteOffset = 1,
            FallbackRva = 0xD2A8D0,
            IsDirectAddress = false
        },

        // Anchor 3: FFX_MinigameTimerPtr (or dword_F2FF14, eax)
        new OffsetBytePattern
        {
            Type = GameOffset.FFX_MinigameTimerPtr,
            Pattern = "09 05 ?? ?? ?? ?? A3 ?? ?? ?? ?? 5B",
            ByteOffset = 2,
            FallbackRva = 0xF2FF14,
            IsDirectAddress = false
        },

        // Anchor 4: FFX_BattlePointerParty (sub_394020, dword_D334D4)
        new OffsetBytePattern
        {
            Type = GameOffset.FFX_BattlePointerParty,
            Pattern = "83 F8 1F 7D 0E 69 C0 90 0F 00 00 03 05 ?? ?? ?? ?? 5D C3",
            ByteOffset = 13,
            FallbackRva = 0xD334D4,
            IsDirectAddress = false
        },

        // Anchor 5: FFX_BattlePointerEnemy (sub_395AA0, dword_D34468)
        new OffsetBytePattern
        {
            Type = GameOffset.FFX_BattlePointerEnemy,
            Pattern = "0F B6 45 08 69 C0 90 0F 00 00 03 05 ?? ?? ?? ?? 5D C3",
            ByteOffset = 12,
            FallbackRva = 0xD34468,
            IsDirectAddress = false
        },

        // Anchor 6: FFX_SphereGrid (sub_6572E0)
        new OffsetBytePattern
        {
            Type = GameOffset.FFX_SphereGrid,
            Pattern = "D9 54 24 14 66 A3 ?? ?? ?? ?? D9 05 ?? ?? ?? ?? 33 C0",
            ByteOffset = 6,
            FallbackRva = 0x12AD860,
            IsDirectAddress = false
        },

        // Anchor 7: FFX_FuncCalcStats (sub_3860F0, Direct Function Prologue)
        new OffsetBytePattern
        {
            Type = GameOffset.FFX_FuncCalcStats,
            Pattern = "55 8B EC 81 EC E4 00 00 00 A1 ?? ?? ?? ?? 33 C5 89 45 FC 8B 45",
            ByteOffset = 0,
            FallbackRva = 0x3860F0,
            IsDirectAddress = true
        },

        // Anchor 8: FFX_FuncSyncEquip (sub_3ABE30, Direct Function Prologue)
        new OffsetBytePattern
        {
            Type = GameOffset.FFX_FuncSyncEquip,
            Pattern = "B8 ?? ?? ?? ?? C6 00 FF",
            ByteOffset = 0,
            FallbackRva = 0x3ABE30,
            IsDirectAddress = true
        },

        // Dynamic Mod: Break Damage Limit (sub_38E630, and ebx, 15F90h; add ebx, 270Fh)
        new OffsetBytePattern
        {
            Type = GameOffset.FFX_DamageLimitPatch,
            Pattern = "81 E3 90 5F 01 00 81 C3 0F 27 00 00",
            ByteOffset = 0,
            FallbackRva = 0x38ECE1,
            IsDirectAddress = true
        },

        // Dynamic Mod: In-Battle HP Limit (sub_38D270, and eax, 15F90h; add eax, 270Fh)
        new OffsetBytePattern
        {
            Type = GameOffset.FFX_HPLimitInBattle,
            Pattern = "25 ?? ?? ?? ?? 05 ?? ?? ?? ?? 50 8B 86 9C 05 00 00",
            ByteOffset = 0,
            FallbackRva = 0x38D2C9,
            IsDirectAddress = true
        },

        // Dynamic Mod: In-Battle MP Limit (sub_38D270, and eax, 2328h; add eax, 3E7h)
        new OffsetBytePattern
        {
            Type = GameOffset.FFX_MPLimitInBattle,
            Pattern = "25 ?? ?? ?? ?? 05 E7 03 00 00 50 8B 86 A0 05 00 00",
            ByteOffset = 0,
            FallbackRva = 0x38D33D,
            IsDirectAddress = true
        },

        // Dynamic Mod: Out-Battle HP Limit (sub_38D270, and eax, 15F90h; add eax, 270Fh)
        new OffsetBytePattern
        {
            Type = GameOffset.FFX_HPLimitOutOfBattle,
            Pattern = "25 ?? ?? ?? ?? 05 ?? ?? ?? ?? 50 6A 00 FF 75 E4",
            ByteOffset = 0,
            FallbackRva = 0x3867D2,
            IsDirectAddress = true
        },

        // Dynamic Mod: Out-Battle MP Limit (sub_38D270, and eax, 2328h; add eax, 3E7h)
        new OffsetBytePattern
        {
            Type = GameOffset.FFX_MPLimitOutOfBattle,
            Pattern = "25 ?? ?? ?? ?? 05 E7 03 00 00 50 6A 00",
            ByteOffset = 0,
            FallbackRva = 0x3867FA,
            IsDirectAddress = true
        },

        // Dynamic Mod: Seymour Menu Skip Check 1 (sub_4A8F40: cmp al, 7; jz ...; cmp ebx, 10000h)
        new OffsetBytePattern
        {
            Type = GameOffset.FFX_SeymourMenuCheck1,
            Pattern = "3C 07 74 24 81 FB",
            ByteOffset = 0,
            FallbackRva = 0x4A8F97,
            IsDirectAddress = true
        },

        new OffsetBytePattern
        {
            Type = GameOffset.FFX_SeymourMenuCheck2,
            Pattern = "3C 07 74 1E 81 FB",
            ByteOffset = 0,
            FallbackRva = 0x4A8FEA,
            IsDirectAddress = true
        },

        // Dynamic In-Battle AP Share Flag (sub_398B60: push 7; push offset byte_1F10EA0)
        new OffsetBytePattern
        {
            Type = GameOffset.FFX_InBattleFlags,
            Pattern = "6A 07 68 ?? ?? ?? ?? E8 ?? ?? ?? ?? 83 C4 08 C3",
            ByteOffset = 3,
            FallbackRva = 0x1F10EA0,
            IsDirectAddress = false
        },
    ];

    static readonly object _lock = new();
    static readonly Dictionary<GameOffset, int> _offsets = new();
    static readonly HashSet<GameOffset> _failedOffsets = new();

    public static int GetOffset(GameOffset type)
    {
        lock (_lock)
        {
            if (_failedOffsets.Contains(type))
            {
                return 0;
            }

            // --- Dynamic Table 100% ---
            switch (type)
            {
                // Group FFX_SaveDataBase
                case GameOffset.FFX_CurrentRoom:
                    return GetOffset(GameOffset.FFX_SaveDataBase) + 0x0000;
                case GameOffset.FFX_AffectionArray:
                    return GetOffset(GameOffset.FFX_SaveDataBase) + 0x002C;
                case GameOffset.FFX_LightningTotalAll:
                    return GetOffset(GameOffset.FFX_SaveDataBase) + 0x03FC;
                case GameOffset.FFX_LightningTotalDodged:
                    return GetOffset(GameOffset.FFX_SaveDataBase) + 0x03FE;
                case GameOffset.FFX_LightningRecordStreak:
                    return GetOffset(GameOffset.FFX_SaveDataBase) + 0x0400;
                case GameOffset.FFX_TempleSeals:
                    return GetOffset(GameOffset.FFX_SaveDataBase) + 0x0C5C;
                case GameOffset.FFX_BlitzballTeamData:
                    return GetOffset(GameOffset.FFX_SaveDataBase) + 0x0C74;
                case GameOffset.FFX_BlitzballData:
                    return GetOffset(GameOffset.FFX_SaveDataBase) + 0x11E8;
                case GameOffset.FFX_AlBhed:
                    return GetOffset(GameOffset.FFX_SaveDataBase) + 0x3D10;
                case GameOffset.FFX_CurrentGil:
                    return GetOffset(GameOffset.FFX_SaveDataBase) + 0x3D48;
                case GameOffset.FFX_PartyList:
                    return GetOffset(GameOffset.FFX_SaveDataBase) + 0x3D58;
                case GameOffset.FFX_OverdriveUnlockBase:
                    return GetOffset(GameOffset.FFX_SaveDataBase) + 0x3D6C;
                case GameOffset.FFX_YojimboCompatibility:
                    return GetOffset(GameOffset.FFX_SaveDataBase) + 0x3DA4;
                case GameOffset.FFX_TidusOverdrive:
                    return GetOffset(GameOffset.FFX_SaveDataBase) + 0x3DAC;
                case GameOffset.FFX_ItemTypes:
                    return GetOffset(GameOffset.FFX_SaveDataBase) + 0x3ECC;
                case GameOffset.FFX_ItemCounts:
                    return GetOffset(GameOffset.FFX_SaveDataBase) + 0x40CC;
                case GameOffset.FFX_MonstersCaptured:
                    return GetOffset(GameOffset.FFX_SaveDataBase) + 0x420C;
                case GameOffset.FFX_KeyItems:
                    return GetOffset(GameOffset.FFX_SaveDataBase) + 0x448C;
                case GameOffset.FFX_EquipmentBase:
                    return GetOffset(GameOffset.FFX_SaveDataBase) + 0x449C;
                case GameOffset.FFX_PartyStatBase:
                    return GetOffset(GameOffset.FFX_SaveDataBase) + 0x55D0;
                case GameOffset.FFX_WakkaBattleCount:
                    return GetOffset(GameOffset.FFX_SaveDataBase) + 0x586C;
                case GameOffset.FFX_AeonNames:
                    return GetOffset(GameOffset.FFX_SaveDataBase) + 0x63EC;

                // Group FFX_BtlBase
                case GameOffset.FFX_BattlePause:
                    return GetOffset(GameOffset.FFX_BtlBase) + 0x0011;
                case GameOffset.FFX_DebugFlags:
                    return GetOffset(GameOffset.FFX_BtlBase) + 0x0028;
                case GameOffset.FFX_CommandBinPointer:
                case GameOffset.FFX_SkillTablePointer:
                    return GetOffset(GameOffset.FFX_BtlBase) + 0x005C;
                case GameOffset.FFX_EnemySkillTablePointer:
                case GameOffset.FFX_MonMagic1BinPointer:
                    return GetOffset(GameOffset.FFX_BtlBase) + 0x0060;
                case GameOffset.FFX_MonMagic2BinPointer:
                    return GetOffset(GameOffset.FFX_BtlBase) + 0x0064;
                case GameOffset.FFX_PlyRomBinPointer:
                    return GetOffset(GameOffset.FFX_BtlBase) + 0x0068;
                case GameOffset.FFX_ItemBinPointer:
                    return GetOffset(GameOffset.FFX_BtlBase) + 0x0070;
                case GameOffset.FFX_AAbilityBinPointer:
                    return GetOffset(GameOffset.FFX_BtlBase) + 0x0074;
                case GameOffset.FFX_SizeCommandBin:
                    return GetOffset(GameOffset.FFX_BtlBase) + 0x0098;
                case GameOffset.FFX_SizePlyRomBin:
                    return GetOffset(GameOffset.FFX_BtlBase) + 0x009A;
                case GameOffset.FFX_SizeItemBin:
                    return GetOffset(GameOffset.FFX_BtlBase) + 0x009E;
                case GameOffset.FFX_SizeAAbilityBin:
                    return GetOffset(GameOffset.FFX_BtlBase) + 0x00A0;

                // Derive Out-of-Battle HP/MP Mod from Anchor FuncCalcStats (sub_3860F0)
                //case GameOffset.FFX_HPLimitOutOfBattle:
                //    return GetOffset(GameOffset.FFX_FuncCalcStats) + 0x6E2; // 0x3867D2
                //case GameOffset.FFX_MPLimitOutOfBattle:
                //    return GetOffset(GameOffset.FFX_FuncCalcStats) + 0x70A; // 0x3867FA

                // Derive post-battle AP flags array (exactly 0x24 = 36 bytes away from InBattleFlags array)
                case GameOffset.FFX_GainedAPFlags:
                    return GetOffset(GameOffset.FFX_InBattleFlags) + 0x24; // 0x1F10EC4

                // Group MinigameTimerPtr & Others
                case GameOffset.FFX_BlitzballPointer:
                    return GetOffset(GameOffset.FFX_MinigameTimerPtr);
                case GameOffset.FFX_BlitzballMode:
                    return GetOffset(GameOffset.FFX_SaveDataBase) + 0x0750;
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
        if (!GameMemory.IsAttached)
        {
            return false;
        }

        try
        {

            var process = GameMemory.Process;
            if (process == null || process.HasExited)
            {
                return false;
            }

            var baseAddress = (int)process.MainModule.BaseAddress;
            _sigScan.Process = process;
            _sigScan.Address = process.MainModule.BaseAddress;
            _sigScan.Size = process.MainModule.ModuleMemorySize;

            var patternData = _bytePatterns.FirstOrDefault(p => p.Type == offsetType);
            if (patternData == null)
            {
                return false;
            }

            var pointer = _sigScan.FindPattern(patternData.Pattern, patternData.ByteOffset);
            var resolvedAddress = 0;

            if (pointer != IntPtr.Zero)
            {
                if (patternData.IsDirectAddress)
                {
                    resolvedAddress = (int)pointer;
                }
                else
                {
                    resolvedAddress = GameMemory.Read<int>((int)pointer, false);
                }
            }

            if (resolvedAddress == 0 && patternData.FallbackRva != 0)
            {
                resolvedAddress = baseAddress + patternData.FallbackRva;
            }

            if (resolvedAddress == 0)
            {
                return false;
            }

            _offsets[patternData.Type] = resolvedAddress;
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OffsetScanner] Exception scanning {offsetType}: {ex.Message}");
            return false;
        }
    }
}

public class OffsetBytePattern
{
    public GameOffset Type { get; set; }
    public string Pattern { get; set; }
    public int ByteOffset { get; set; }
    public int FallbackRva { get; set; }
    public bool IsDirectAddress { get; set; }
}

public enum GameOffset
{
    // Anchors
    FFX_SaveDataBase,
    FFX_BtlBase,
    FFX_MinigameTimerPtr,
    FFX_BattlePointerParty,
    FFX_BattlePointerEnemy,
    FFX_SphereGrid,
    FFX_FuncCalcStats,
    FFX_FuncSyncEquip,

    // Core Game Offsets
    FFX_ItemTypes,
    FFX_ItemCounts,
    FFX_PartyStatBase,
    FFX_EquipmentBase,
    FFX_KeyItems,
    FFX_AlBhed,
    FFX_CurrentGil,
    FFX_TidusOverdrive,
    FFX_PartyList,
    FFX_MonstersCaptured,
    FFX_AeonNames,
    FFX_InBattleFlags,
    FFX_GainedAPFlags,
    FFX_CurrentRoom,
    FFX_DebugFlags,
    FFX_BlitzballData,
    FFX_BlitzballPointer,
    FFX_BlitzballMode,
    FFX_BlitzballTeamData,
    FFX_SkillTablePointer,
    FFX_EnemySkillTablePointer,

    // Mod Specific Offsets (AOB Dynamic)
    FFX_DamageLimitPatch,
    FFX_HPLimitInBattle,
    FFX_MPLimitInBattle,
    FFX_HPLimitOutOfBattle,
    FFX_MPLimitOutOfBattle,
    FFX_SeymourMenuCheck1,
    FFX_SeymourMenuCheck2,

    // Derived Detail Offsets
    FFX_BattlePause,
    FFX_CommandBinPointer,
    FFX_ItemBinPointer,
    FFX_MonMagic1BinPointer,
    FFX_MonMagic2BinPointer,
    FFX_PlyRomBinPointer,
    FFX_AAbilityBinPointer,
    FFX_SizeCommandBin,
    FFX_SizePlyRomBin,
    FFX_SizeItemBin,
    FFX_SizeAAbilityBin,
    FFX_AffectionArray,
    FFX_TempleSeals,
    FFX_OverdriveUnlockBase,
    FFX_YojimboCompatibility,
    FFX_WakkaBattleCount,
    FFX_LightningTotalAll,
    FFX_LightningTotalDodged,
    FFX_LightningRecordStreak,
}
