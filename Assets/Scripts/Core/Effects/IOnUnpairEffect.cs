using CardBattle.AI;
using CardBattle.Core.Field;

namespace CardBattle.Core.Effects
{
    /// <summary>
    /// ペアリング解除時に発動する効果のインターフェース。
    /// </summary>
    public interface IOnUnpairEffect
    {
        /// <summary>
        /// ペアリング解除時の効果を適用する。
        /// </summary>
        /// <param name="leavingUnit">場を離れるユニット（破壊・除去される側）</param>
        /// <param name="state">現在のゲーム状態（除去前の状態で呼ばれる）</param>
        /// <param name="myUnit">効果を持つ側のユニット（leavingUnit のペア相手、または leavingUnit 自身）</param>
        void Resolve(Unit leavingUnit, GameState state, Unit myUnit);
    }
}
