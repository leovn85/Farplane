using Farplane.FFX.Values;
using Farplane.Memory;

namespace Farplane.FFX.Data;

public static class Battle
{
    static int _offsetEnemyPointer => OffsetScanner.GetOffset(GameOffset.FFX_BattlePointerEnemy);
    static int _offsetPartyPointer => OffsetScanner.GetOffset(GameOffset.FFX_BattlePointerParty);

    public const int BlockLengthEntity = 0xF90;

    public static bool GetBattleState()
    {
        var battlePointer = GameMemory.Read<uint>(_offsetPartyPointer, false);
        return battlePointer != 0;
    }

    public static BattleEntityData GetPartyEntity(int entityIndex)
    {
        BattleEntity.ReadEntity(EntityType.Party, entityIndex, out var entityData);
        return entityData;
    }

    public static BattleEntityData GetEnemyEntity(int entityIndex)
    {
        BattleEntity.ReadEntity(EntityType.Enemy, entityIndex, out var entityData);
        return entityData;
    }
}
