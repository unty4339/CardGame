using System.Collections.Generic;
using CardBattle.AI;
using CardBattle.Core.Effects;
using CardBattle.Core.Field;
using UnityEngine;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// ペアリング効果を持つユニットカードの例。
    /// ペア対象: HP1の敵ユニット / パートナーユニット / パートナーカードのいずれかを選択できる。
    /// </summary>
    public class PairingUnitCard : UnitCardTemplateBase, IOnPairingEffect
    {
        public PairingUnitCard()
        {
            cardName = "ペアリングの従者";
            playCost = 2;
            baseHP = 2;
            baseAttack = 1;
            description = "登場時、HP1の敵ユニット1体、またはパートナーユニット、またはパートナーカードをペア対象に選ぶ。";
        }

        public IList<EffectTarget> GetAvailableTargets(GameState state, Unit sourceUnit, bool isPartnerOnField)
        {
            return PairingTargetCandidates.GetStandardPairingTargets(state, isPartnerOnField);
        }

        public void Resolve(EffectTarget target, GameState state, Unit sourceUnit, Unit pairTargetUnitOrNull)
        {
            if (pairTargetUnitOrNull != null)
            {
                // ペアリング時効果の例: ペア対象の攻撃力を参照して何かする（ここでは何もしない）
                _ = pairTargetUnitOrNull.Attack;
            }
        }
    }
}
