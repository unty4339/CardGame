using System.Collections.Generic;
using CardBattle.AI;
using CardBattle.Core.Effects;
using CardBattle.Core.Enums;
using CardBattle.Core.Field;
using CardBattle.UI;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// 肉鎧のオーク。登場時ペア。ペアリング中：攻撃できない付与、戦闘時身代わり。
    /// </summary>
    public class FleshArmorOgreUnitCard : UnitCardTemplateBase, IOnPairingEffect, IGrantsCannotAttackToPairTarget, IWhilePairedSubstitution, IPairingStandingPicture
    {
        public FleshArmorOgreUnitCard()
        {
            cardName = "肉鎧のオーク";
            playCost = 7;
            baseHP = 3;
            baseAttack = 5;
            description = "登場時：\nペアリング中でない、自分パートナーか体力1の相手ユニットとペアリングする。\nペアリング中：\nペア対象に「攻撃できない」を付与する。\nこのカードが戦闘を行うとき、ダメージを受ける代わりにペア対象を破壊する。";
        }

        public IList<EffectTarget> GetAvailableTargets(GameState state, Unit sourceUnit, bool isPartnerOnField)
        {
            return PairingTargetCandidates.GetStandardPairingTargets(state, isPartnerOnField);
        }

        public void Resolve(EffectTarget target, GameState state, Unit sourceUnit, Unit pairTargetUnitOrNull)
        {
            if (pairTargetUnitOrNull == null) return;
            pairTargetUnitOrNull.CanAttackUnit = false;
            pairTargetUnitOrNull.CanAttackPlayer = false;
        }

        public string GetStandingPictureTypeWhenPartnerChosen(EffectTarget target, Unit pairTargetUnitOrNull)
        {
            var isPartnerChosen = (target.Kind == EffectTargetKind.PartnerCard && target.PlayerId == 0)
                || (pairTargetUnitOrNull != null && pairTargetUnitOrNull.IsPartner && pairTargetUnitOrNull.OwnerPlayerId == 0);
            return isPartnerChosen ? StandingPictureType.Ogre : null;
        }
    }
}
