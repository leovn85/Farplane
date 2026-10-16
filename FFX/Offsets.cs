using System.Collections.Generic;
using Farplane.FFX.Values;

namespace Farplane.FFX;

public static class Offsets
{
    static readonly Dictionary<OffsetType, int> _offsetList = new()
    {
        { OffsetType.EquipmentBase, 0xD30F2C },
        { OffsetType.PartyStatsBase, 0xD32060 },
        { OffsetType.ItemTypes, 0xD3095C },
        { OffsetType.ItemCounts, 0xD30B5C },
        { OffsetType.DebugFlags, 0xD2A8F8 },
        { OffsetType.PartyInBattleFlags, 0x1F10EA0 },
        { OffsetType.PartyGainedApFlags, 0x1F10EC4 },
        { OffsetType.AeonNames, 0xD32E7C },
        { OffsetType.KeyItems, 0xD30F1C },
        { OffsetType.AlBhed, 0xD307A0 },
        { OffsetType.SphereGridNodes, 0x12AE078 },
        { OffsetType.SphereGridCursor, 0x12BEB6C },
        { OffsetType.CurrentGil, 0xD307D8 },
        { OffsetType.BattleEnemyPointer, 0xD34468 },
        { OffsetType.BattlePlayerPointer, 0xD334D4 },
        { OffsetType.MonsterArena, 0xD30C9C },
        { OffsetType.BlitzballDataPointer, 0x384670 },
        { OffsetType.BlitzballGamePointer, 0xF2FF14 },
        { OffsetType.BlitzballTeamSizes, 0xD2D704 },
        { OffsetType.TidusOverdrive, 0xD3083C },
        { OffsetType.PartyList, 0xD307E8 },
        { OffsetType.CurrentRoom, 0xD2CA90 },
        { OffsetType.BattlePause, 0xD2A8E1 },
        { OffsetType.SaveDataBase, 0xD2CA90 },
        { OffsetType.BtlBase, 0xD2A8D0 },
    };

    public static int GetOffset(OffsetType offsetType)
    {
        var offset = _offsetList[offsetType];
        return offset;
    }
}

public enum OffsetType
{
    EquipmentBase,
    PartyStatsBase,
    ItemTypes,
    ItemCounts,
    DebugFlags,
    PartyInBattleFlags,
    PartyGainedApFlags,
    AeonNames,
    KeyItems,
    AlBhed,
    SphereGridNodes,
    SphereGridCursor,
    CurrentGil,
    BattleEnemyPointer,
    BattlePlayerPointer,
    RemoveDamageLimit,
    RemoveHPLimit,
    RemoveMPLimit,
    MonsterArena,
    BlitzballDataPointer,
    BlitzballGamePointer,
    BlitzballTeamSizes,
    RemoveMPCheck,
    RemoveHPCheck,
    TidusOverdrive,
    PartyList,
    CurrentRoom,
    BattlePause,
    SaveDataBase,
    BtlBase,
}

public enum EquipmentOffset
{
    Name = 0,
    DamageFormula = 2,
    Character = 4,
    Type = 5,
    AbilityCount = 0xB,
    Appearance = 0x0C,
    Ability0 = 0xE,
    Ability1 = 0x10,
    Ability2 = 0x12,
    Ability3 = 0x14,
}

public enum BlitzballDataOffset
{
    EquippedTechs = 0x266,
    TechCount = 0x392,
    PlayerLevels = 0x3CE,
    TeamData = 0x41a,
    ContractLength = 0x52a,
    PlayerEXP = 0x568,
}

public enum BlockLength
{
    SphereGridNode = 0x28,
    BattleEntity = 0xF90,
    MonsterArenaCount = 0x8B,
    EquipmentItem = 0x16,
    SkillFlags = 0xD,
}
