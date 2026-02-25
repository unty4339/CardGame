using System.Collections.Generic;
using CardBattle.AI;
using CardBattle.Core.Effects;
using CardBattle.Core.Enums;
using CardBattle.Core.Field;
using CardBattle.Managers;
using CardBattle.UI;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// 肉鎧のオーク。登場時ペア、ペアリング時+X/+X、ペアリング中攻撃できない付与済み、戦闘時身代わり（ペア先破壊 or ペア解除でダメージ無効）。
    /// </summary>
    public class FleshArmorOgreUnitCard : UnitCardTemplateBase, IOnPairingEffect, IGrantsCannotAttackToPairTarget, IWhilePairedSubstitution, IPairingStandingPicture
    {
        public FleshArmorOgreUnitCard()
        {
            cardName = "肉鎧のオーク";
            playCost = 7;
            baseHP = 5;
            baseAttack = 3;
            description = "登場時：\nペアリング中でない、自分パートナーか体力1の相手ユニットとペアリングする。\n\nペアリング時：\nこれを+X/Xする。Xはペア対象の攻撃力となる。\n\nペアリング中：\nペア対象に「攻撃できない」を付与する。\n\n破壊時：\nこのカードが戦闘で破壊されるとき、代わりにペア対象を破壊する。";
        }

        public IList<EffectTarget> GetAvailableTargets(GameState state, Unit sourceUnit, bool isPartnerOnField)
        {
            return PairingTargetCandidates.GetStandardPairingTargets(state, isPartnerOnField);
        }

        public void Resolve(EffectTarget target, GameState state, Unit sourceUnit, Unit pairTargetUnitOrNull)
        {
            if (sourceUnit == null || pairTargetUnitOrNull == null) return;
            var x = pairTargetUnitOrNull.Attack;
            PlayerManager.Instance?.AddUnitAttack(sourceUnit, x);
            PlayerManager.Instance?.AddUnitHp(sourceUnit, x);
            sourceUnit.PairingAttackBonus = x;
            sourceUnit.PairingHpBonus = x;
            pairTargetUnitOrNull.CanAttackUnit = false;
            pairTargetUnitOrNull.CanAttackPlayer = false;
        }

        public string GetStandingPictureTypeWhenPartnerChosen(EffectTarget target, Unit pairTargetUnitOrNull)
        {
            var isPartnerChosen = (target.Kind == EffectTargetKind.PartnerCard && target.PlayerId == 0)
                || (pairTargetUnitOrNull != null && pairTargetUnitOrNull.IsPartner && pairTargetUnitOrNull.OwnerPlayerId == 0);
            return isPartnerChosen ? StandingPictureType.Restraint : null;
        }
    }
}
