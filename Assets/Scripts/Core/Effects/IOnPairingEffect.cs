using System.Collections.Generic;
using CardBattle.AI;
using CardBattle.Core.Field;

namespace CardBattle.Core.Effects
{
    /// <summary>
    /// ペアリング時に発動する効果のインターフェース。
    /// ペア対象候補は「HP1の敵ユニット」「パートナーユニット」「パートナーカード」など。isPartnerOnField でパートナー状態を渡す。
    /// </summary>
    public interface IOnPairingEffect
    {
        /// <summary>
        /// ペアリング対象として選択可能な対象の一覧を返す。
        /// </summary>
        /// <param name="state">現在のゲーム状態</param>
        /// <param name="sourceUnit">ペアリングを発動するユニット（召喚されたばかりのユニット）</param>
        /// <param name="isPartnerOnField">自プレイヤーのパートナーが既にフィールドにユニットとして出ているか</param>
        IList<EffectTarget> GetAvailableTargets(GameState state, Unit sourceUnit, bool isPartnerOnField);

        /// <summary>
        /// ペアリングを解決する。target に応じて PairingTarget または PairingWithPartnerCard を設定し、必要なら OnPairing 時の処理を行う。
        /// Unit を選んだ場合は呼び出し側で双方向に PairingTarget を設定した後に呼ばれる。PartnerCard の場合は PairingWithPartnerCard 設定後に呼ばれる。
        /// </summary>
        void Resolve(EffectTarget target, GameState state, Unit sourceUnit, Unit pairTargetUnitOrNull);
    }
}
