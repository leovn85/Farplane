using Farplane;
using Farplane.Common;
using Farplane.FarplaneMod;
using Farplane.FFX;
using Farplane.FFX.Data;
using Farplane.FFX.Values;
using Farplane.Memory;
using System;
using System.ComponentModel;
using System.Linq;

public class FFXHardBattleMod : IFarplaneMod
{
    private bool _modActive = false;
    private static int OffsetEnemyPointer => Offsets.GetOffset(OffsetType.BattleEnemyPointer);
    private static readonly int SizeBattleEntity = StructHelper.GetSize<BattleEntityData>();
    private static bool changedThisBattle;
    private static bool[] changedCreature;

    public int m_HpMpMultiplier = 10;
    public int m_StatMultiplier = 3;

    public void Configure(object parentWindow) { }
    public string ConfigButton => null;
    public bool AutoActivate => true;
    public string Name => "Harder Battles";
    public string Author => "Topher";
    public string Description => "Multiplies enemy stats at the start of every battle.";
    public GameType GameType => GameType.FFX;

    public ModState GetState() => _modActive ? ModState.Activated : ModState.Deactivated;

    public void Activate()
    {
        if (_modActive)
            return;
        ModLogger.WriteLine("Harder battle mod activated");
        _modActive = true;
    }

    public void Deactivate()
    {
        if (!_modActive)
            return;
        ModLogger.WriteLine("Harder battle mod deactivated");
        _modActive = false;
    }

    public void Update()
    {
        if (!_modActive || !GameMemory.IsAttached)
            return;

        var battlePointer = GameMemory.Read<IntPtr>(OffsetEnemyPointer);

        if (battlePointer == IntPtr.Zero && changedCreature != null)
        {
            changedThisBattle = false;
            changedCreature = null;
        }

        if (battlePointer != IntPtr.Zero && !changedThisBattle)
        {
            ModLogger.WriteLine("Battle detected, modifying enemies");
            changedCreature = new bool[8];
            var loopLimit = 2000;

            while (changedCreature.Contains(false) && loopLimit > 0)
            {
                loopLimit--;
                for (var i = 0; i < 8; i++)
                {
                    if (changedCreature[i])
                        continue;

                    var entityData = Battle.GetEnemyEntity(i);
                    var entityOffset = (int)battlePointer + (SizeBattleEntity * i);
                    if (entityData.pointer_1 == 0)
                        continue;

                    var entityName = StringConverter.ToASCII(entityData.text_name);
                    ModLogger.WriteLine("Modifying creature: {0}", entityName);

                    var newHP = (long)entityData.hp_max * m_HpMpMultiplier;
                    newHP = newHP > int.MaxValue ? int.MaxValue : newHP;

                    var newMP = (long)entityData.mp_max * m_HpMpMultiplier;
                    newMP = newMP > int.MaxValue ? int.MaxValue : newMP;

                    var newStrength = Math.Min(byte.MaxValue, entityData.strength * m_StatMultiplier);
                    var newDefense = Math.Min(byte.MaxValue, entityData.defense * m_StatMultiplier);
                    var newMagic = Math.Min(byte.MaxValue, entityData.magic * m_StatMultiplier);
                    var newMagicDef = Math.Min(byte.MaxValue, entityData.magic_defense * m_StatMultiplier);
                    var newAgility = Math.Min(byte.MaxValue, entityData.agility * m_StatMultiplier);
                    var newLuck = Math.Min(byte.MaxValue, entityData.luck * m_StatMultiplier);
                    var newAccuracy = Math.Min(byte.MaxValue, entityData.accuracy * m_StatMultiplier);
                    var newEvasion = Math.Min(byte.MaxValue, entityData.evasion * m_StatMultiplier);

                    // Write stats with isRelative: false as entityOffset is an absolute pointer
                    GameMemory.Write(entityOffset + StructHelper.GetFieldOffset<BattleEntityData>(nameof(BattleEntityData.hp_current)), (int)newHP, false);
                    GameMemory.Write(entityOffset + StructHelper.GetFieldOffset<BattleEntityData>(nameof(BattleEntityData.hp_max)), (int)newHP, false);
                    GameMemory.Write(entityOffset + StructHelper.GetFieldOffset<BattleEntityData>(nameof(BattleEntityData.hp_max2)), (int)newHP, false);
                    GameMemory.Write(entityOffset + StructHelper.GetFieldOffset<BattleEntityData>(nameof(BattleEntityData.mp_current)), (int)newMP, false);
                    GameMemory.Write(entityOffset + StructHelper.GetFieldOffset<BattleEntityData>(nameof(BattleEntityData.mp_max)), (int)newMP, false);
                    GameMemory.Write(entityOffset + StructHelper.GetFieldOffset<BattleEntityData>(nameof(BattleEntityData.mp_max2)), (int)newMP, false);

                    GameMemory.Write(entityOffset + StructHelper.GetFieldOffset<BattleEntityData>(nameof(BattleEntityData.strength)), (byte)newStrength, false);
                    GameMemory.Write(entityOffset + StructHelper.GetFieldOffset<BattleEntityData>(nameof(BattleEntityData.defense)), (byte)newDefense, false);
                    GameMemory.Write(entityOffset + StructHelper.GetFieldOffset<BattleEntityData>(nameof(BattleEntityData.magic)), (byte)newMagic, false);
                    GameMemory.Write(entityOffset + StructHelper.GetFieldOffset<BattleEntityData>(nameof(BattleEntityData.magic_defense)), (byte)newMagicDef, false);
                    GameMemory.Write(entityOffset + StructHelper.GetFieldOffset<BattleEntityData>(nameof(BattleEntityData.agility)), (byte)newAgility, false);
                    GameMemory.Write(entityOffset + StructHelper.GetFieldOffset<BattleEntityData>(nameof(BattleEntityData.luck)), (byte)newLuck, false);
                    GameMemory.Write(entityOffset + StructHelper.GetFieldOffset<BattleEntityData>(nameof(BattleEntityData.accuracy)), (byte)newAccuracy, false);
                    GameMemory.Write(entityOffset + StructHelper.GetFieldOffset<BattleEntityData>(nameof(BattleEntityData.evasion)), (byte)newEvasion, false);

                    changedCreature[i] = true;
                }
            }
            changedThisBattle = true;
        }
    }
}
