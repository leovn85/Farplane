using Farplane.FFX.Values;
using Farplane.Memory;

namespace Farplane.FFX.Data;

public static class Battle
{
    public const int BlockLengthEntity = 0xF90; // 3984 bytes

    public static bool GetBattleState() => BattleEntity.CheckBattleState();

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
