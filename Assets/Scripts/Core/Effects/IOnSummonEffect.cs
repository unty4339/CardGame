using System.Collections.Generic;
using CardBattle.AI;
using CardBattle.Core.Field;

namespace CardBattle.Core.Effects
{
    /// <summary>
    /// 召喚時に発動する効果のインターフェース
    /// </summary>
    public interface IOnSummonEffect
    {
        /// <summary>
        /// 選択可能な対象の一覧を返す。シミュレーション用 GameState または実データ参照の GameState の両方で使用可能。
        /// </summary>
        IList<EffectTarget> GetAvailableTargets(GameState state, Unit sourceUnit);

        /// <summary>
        /// 効果を適用する。state のフィールドを直接書き換える。実プレイ時は解決後に HP 等を PlayerManager に同期すること。
        /// </summary>
        void Resolve(EffectTarget target, GameState state, Unit sourceUnit);
    }
}
