using CardBattle.AI;
using CardBattle.Core.Field;

namespace CardBattle.Core.Effects
{
    /// <summary>
    /// 破壊時（デスラトル）に発動する効果のインターフェース。将来の拡張用。
    /// </summary>
    public interface IDeathrattleEffect
    {
        void Resolve(GameState state, Unit destroyedUnit);
    }
}
