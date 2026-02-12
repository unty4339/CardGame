using System.Collections.Generic;
using CardBattle.AI;

namespace CardBattle.Core.Effects
{
    /// <summary>
    /// 呪文プレイ時に発動する効果のインターフェース
    /// </summary>
    public interface ISpellEffect
    {
        /// <summary>
        /// 選択可能な対象の一覧を返す。
        /// </summary>
        IList<EffectTarget> GetAvailableTargets(GameState state);

        /// <summary>
        /// 効果を適用する。state のフィールドを直接書き換える。
        /// </summary>
        void Resolve(EffectTarget target, GameState state);
    }
}
