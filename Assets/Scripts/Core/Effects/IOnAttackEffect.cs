using CardBattle.AI;
using CardBattle.Core.Field;

namespace CardBattle.Core.Effects
{
    /// <summary>
    /// 攻撃時に発動する効果のインターフェース。将来の拡張用。
    /// </summary>
    public interface IOnAttackEffect
    {
        void OnAttack(GameState state, Unit attacker, EffectTarget attackTarget);
    }
}
