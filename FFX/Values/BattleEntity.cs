using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Farplane.Common;
using Farplane.FFX.Data;
using Farplane.Memory;

namespace Farplane.FFX.Values;

public class BattleEntity
{
    public static int OffsetPtrParty => OffsetScanner.GetOffset(GameOffset.FFX_BattlePointerParty);
    public static int OffsetPtrEnemy => OffsetScanner.GetOffset(GameOffset.FFX_BattlePointerEnemy);
    public static readonly FieldInfo[] StructInfo = typeof(BattleEntityData).GetFields();

    public static bool ReadEntity(
        EntityType entityType,
        int entityIndex,
        out BattleEntityData outputEntity
    )
    {
        var offsetEntity = GetEntityOffset(entityType, entityIndex);
        if (offsetEntity == 0)
        {
            outputEntity = default;
            return false;
        }

        var entityData = GameMemory.Read<byte>(offsetEntity, Battle.BlockLengthEntity, false);
        if (entityData == null || entityData.Length < Battle.BlockLengthEntity)
        {
            outputEntity = default;
            return false;
        }

        outputEntity = StructHelper.BytesToStruct<BattleEntityData>(entityData);
        return true;
    }

    public static bool CheckBattleState()
    {
        var ptrParty = GameMemory.Read<IntPtr>(OffsetPtrParty, false);
        return ptrParty != IntPtr.Zero;
    }

    public static int GetEntityOffset(EntityType entityType, int entityIndex)
    {
        var pointerOffset = entityType == EntityType.Party ? OffsetPtrParty : OffsetPtrEnemy;
        if (pointerOffset == 0)
        {
            return 0;
        }

        var ptrEntityList = GameMemory.Read<IntPtr>(pointerOffset, false);
        if (ptrEntityList == IntPtr.Zero)
        {
            return 0;
        }

        return (int)ptrEntityList + (Battle.BlockLengthEntity * entityIndex);
    }

    public static void WriteBytes(
        EntityType entityType,
        int entityIndex,
        string entityDataType,
        byte[] dataToWrite
    )
    {
        var entityOffset = GetEntityOffset(entityType, entityIndex);
        if (entityOffset == 0 || dataToWrite == null || dataToWrite.Length == 0)
        {
            return;
        }

        var dataOffset = entityOffset + (int)Marshal.OffsetOf<BattleEntityData>(entityDataType);
        GameMemory.Write(dataOffset, dataToWrite, false);
    }

    public static void WriteBytes(
        EntityType entityType,
        int entityIndex,
        string entityDataType,
        byte dataToWrite
    )
    {
        var entityOffset = GetEntityOffset(entityType, entityIndex);
        if (entityOffset == 0)
        {
            return;
        }

        var dataOffset = entityOffset + (int)Marshal.OffsetOf<BattleEntityData>(entityDataType);
        GameMemory.Write(dataOffset, dataToWrite, false);
    }
}

public enum EntityType
{
    Party,
    Enemy,
}

[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi, Size = 0xF90)]
public struct BattleEntityData
{
    // 0x000 - 0x00C
    [MarshalAs(UnmanagedType.I4)] public int pointer_1;
    public int index_1;
    public int index_2;

    // 0x00C - 0x048 (60 bytes)
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)] public byte[] unknown_1;

    // 0x048 - 0x050
    public int pointer_2;
    public int pointer_3;

    // 0x050 - 0x390 (Strings & text buffers)
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)] public byte[] text_name;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)] public byte[] text_help;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)] public byte[] text_scan;

    // 0x390 - 0x3E8 (88 bytes)
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 88)] public byte[] unknown_2;

    // 0x3E8 - 0x3F6
    [MarshalAs(UnmanagedType.U1)] public byte unknown_9;
    [MarshalAs(UnmanagedType.U1)] public byte unknown_10;
    [MarshalAs(UnmanagedType.U1)] public byte animation_start;
    [MarshalAs(UnmanagedType.U1)] public byte unknown_11;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)] public byte[] unknown_12;
    [MarshalAs(UnmanagedType.U1)] public byte animation_speed;

    // 0x3F6 - 0x594 (414 bytes padding to reach combat stats)
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 414)] public byte[] unknown_8;

    // 0x594 - 0x5A8 (Vitals)
    [MarshalAs(UnmanagedType.U4)] public int hp_max;
    [MarshalAs(UnmanagedType.U4)] public int mp_max;
    [MarshalAs(UnmanagedType.U4)] public int hp_max2;
    [MarshalAs(UnmanagedType.U4)] public int mp_max2;
    [MarshalAs(UnmanagedType.U4)] public int overkill;

    // 0x5A8 - 0x5B0 (Base attributes)
    [MarshalAs(UnmanagedType.U1)] public byte strength;
    [MarshalAs(UnmanagedType.U1)] public byte defense;
    [MarshalAs(UnmanagedType.U1)] public byte magic;
    [MarshalAs(UnmanagedType.U1)] public byte magic_defense;
    [MarshalAs(UnmanagedType.U1)] public byte agility;
    [MarshalAs(UnmanagedType.U1)] public byte luck;
    [MarshalAs(UnmanagedType.U1)] public byte evasion;
    [MarshalAs(UnmanagedType.U1)] public byte accuracy;

    // 0x5B0 - 0x5B8 (Battle Stat Ups)
    [MarshalAs(UnmanagedType.U1)] public byte strength_up;
    [MarshalAs(UnmanagedType.U1)] public byte defense_up;
    [MarshalAs(UnmanagedType.U1)] public byte magic_up;
    [MarshalAs(UnmanagedType.U1)] public byte magic_defense_up;
    [MarshalAs(UnmanagedType.U1)] public byte agility_up;
    [MarshalAs(UnmanagedType.U1)] public byte luck_up;
    [MarshalAs(UnmanagedType.U1)] public byte evasion_up;
    [MarshalAs(UnmanagedType.U1)] public byte accuracy_up;

    // 0x5B8 - 0x5BC (4 bytes)
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] unknown_3;

    // 0x5BC - 0x5BE (Overdrive charge)
    [MarshalAs(UnmanagedType.U1)] public byte overdrive_current;
    [MarshalAs(UnmanagedType.U1)] public byte overdrive_max;

    // 0x5BE - 0x5C8 (10 bytes)
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)] public byte[] unknown_4;

    // 0x5C8 - 0x5C9 (Doom counter)
    [MarshalAs(UnmanagedType.U1)] public byte timer_doom;

    // 0x5C9 - 0x5D0 (7 bytes)
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)] public byte[] unknown_14;

    // 0x5D0 - 0x5D8 (Current HP & MP)
    [MarshalAs(UnmanagedType.U4)] public int hp_current;
    [MarshalAs(UnmanagedType.U4)] public int mp_current;

    // 0x5D8 - 0x606 (46 bytes)
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 46)] public byte[] unknown_5;

    // 0x606 - 0x608 (Negative status flags)
    [MarshalAs(UnmanagedType.U2)] public ushort status_flags_negative;

    // 0x608 - 0x616 (Turns left for temporary statuses, 14 bytes)
    [MarshalAs(UnmanagedType.U1)] public byte status_turns_sleep;
    [MarshalAs(UnmanagedType.U1)] public byte status_turns_silence;
    [MarshalAs(UnmanagedType.U1)] public byte status_turns_darkness;
    [MarshalAs(UnmanagedType.U1)] public byte status_shell;
    [MarshalAs(UnmanagedType.U1)] public byte status_protect;
    [MarshalAs(UnmanagedType.U1)] public byte status_reflect;
    [MarshalAs(UnmanagedType.U1)] public byte status_nultide;
    [MarshalAs(UnmanagedType.U1)] public byte status_nulblaze;
    [MarshalAs(UnmanagedType.U1)] public byte status_nulshock;
    [MarshalAs(UnmanagedType.U1)] public byte status_nulfrost;
    [MarshalAs(UnmanagedType.U1)] public byte status_regen;
    [MarshalAs(UnmanagedType.U1)] public byte status_haste;
    [MarshalAs(UnmanagedType.U1)] public byte status_slow;
    [MarshalAs(UnmanagedType.U1)] public byte status_unknown;

    // 0x616 - 0x618 (Positive & Special status flags)
    [MarshalAs(UnmanagedType.U2)] public ushort status_flags_positive;

    // 0x618 - 0x65C (68 bytes padding to CTB)
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 68)] public byte[] unknown_buff_padding;

    // 0x65C - 0x664 (CTB & Buff Stacks)
    [MarshalAs(UnmanagedType.U1)] public byte ctb;
    [MarshalAs(UnmanagedType.U1)] public byte max_ctb;
    [MarshalAs(UnmanagedType.U1)] public byte cheer_stacks;
    [MarshalAs(UnmanagedType.U1)] public byte aim_stacks;
    [MarshalAs(UnmanagedType.U1)] public byte focus_stacks;
    [MarshalAs(UnmanagedType.U1)] public byte reflex_stacks;
    [MarshalAs(UnmanagedType.U1)] public byte luck_stacks;
    [MarshalAs(UnmanagedType.U1)] public byte jinx_stacks;

    // 0x664 - 0x6E4 (128 bytes padding)
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)] public byte[] unknown_disp_padding;

    // 0x6E4 - 0x6F0 (Live gauges)
    [MarshalAs(UnmanagedType.U4)] public int current_hp_display;
    [MarshalAs(UnmanagedType.U4)] public int current_mp_display;
    [MarshalAs(UnmanagedType.U4)] public int current_ctb_display;

    // 0x6F0 - 0x704 (20 bytes padding)
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)] public byte[] unknown_bribe_padding;

    // 0x704 - 0x708 (Bribe Gil)
    [MarshalAs(UnmanagedType.U4)] public uint bribe_gil_spent;

    // 0x708 - 0xDEB (1763 bytes padding to Steal Count)
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1763)] public byte[] unknown_steal_padding;

    // 0xDEB - 0xDEC (Steal Count)
    [MarshalAs(UnmanagedType.U1)] public byte steal_count;

    // 0xDEC - 0xF88 (412 bytes padding to Loot Pointer)
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 412)] public byte[] unknown_loot_padding;

    // 0xF88 - 0xF8C (Monster Loot Pointer)
    [MarshalAs(UnmanagedType.I4)] public int ptr_monster_loot;

    // 0xF8C - 0xF90 (4 bytes ending alignment)
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] unknown_end_padding;
}
