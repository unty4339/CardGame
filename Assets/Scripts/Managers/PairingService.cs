using System.Collections.Generic;
using CardBattle.AI;
using CardBattle.Core.Effects;
using CardBattle.Core.Enums;
using CardBattle.Core.Field;
using CardBattle.Core.Player;
using CardBattle.ScriptableObjects;
using CardBattle.UI;

namespace CardBattle.Managers
{
    /// <summary>
    /// ペアリングの解決・解除・ボーナス還元を一箇所で行う。
    /// </summary>
    public static class PairingService
    {
        /// <summary>
        /// ペアリングで加算した攻撃・体力ボーナスを還元し、保存値をクリアする。
        /// </summary>
        public static void ApplyAndClearPairingBonus(Unit unit)
        {
            if (unit == null) return;
            var pm = PlayerManager.Instance;
            if (pm == null) return;
            pm.AddUnitAttack(unit, -unit.PairingAttackBonus);
            pm.AddUnitHp(unit, -unit.PairingHpBonus);
            unit.PairingAttackBonus = 0;
            unit.PairingHpBonus = 0;
        }

        /// <summary>
        /// パートナーカードとのペアリングのみ解除する（破壊通知は行わない）。
        /// </summary>
        public static void UnpairPartnerCardOnly(Unit unit)
        {
            if (unit == null || !unit.PairingWithPartnerCard)
                return;

            var pm = PlayerManager.Instance;
            if (pm == null) return;

            // 先にフラグを解除しておく。ApplyAndClearPairingBonus 内で NotifyUnitAttackChanged が飛び、
            // 表示が GetEffectiveAttack(unit) で更新されるため、この時点でペア解除済みにしておく必要がある。
            unit.PairingWithPartnerCard = false;

            ApplyAndClearPairingBonus(unit);

            var state = pm.GetGameStateForPlayer(unit.OwnerPlayerId);
            var template = unit.SourceCardTemplate;
            if (template != null)
            {
                foreach (var effect in template.GetOnUnpairEffects())
                    effect.Resolve(unit, state, unit);
            }
            GameVisualManager.Instance?.UpdatePartnerCardDraggable(unit.OwnerPlayerId);

            // パートナー選択時に変更した立ち絵を通常に戻す（プレイヤー0のユニットがペア解除した場合）
            if (unit.OwnerPlayerId == 0)
                StandingPictureManager.Instance?.SetStandingPicture(StandingPictureType.Normal);
        }

        /// <summary>
        /// ユニットが場を離れる前にペアリング解除（OnUnpair 発動・参照クリア）を行い、続けて破壊通知する。
        /// 呼び出し元はこのメソッドの後に Units.Remove(unit) を行うこと。
        /// </summary>
        public static void UnpairAndNotifyDestroyed(Unit unit, UnitDestroyReason reason = UnitDestroyReason.Battle)
        {
            var pm = PlayerManager.Instance;
            if (pm == null) return;

            if (unit == null)
            {
                pm.NotifyUnitDestroyed(unit, reason);
                return;
            }

            var needPartnerSweating = (unit.IsPartner && unit.OwnerPlayerId == 0)
                || (unit.IsPairedWithUnit && unit.GetPairTargetUnitOrNull().IsPartner && unit.GetPairTargetUnitOrNull().OwnerPlayerId == 0)
                || (unit.PairingWithPartnerCard && unit.OwnerPlayerId == 0);

            if (unit.PairingWithPartnerCard)
            {
                ApplyAndClearPairingBonus(unit);
                var state = pm.GetGameStateForPlayer(unit.OwnerPlayerId);
                var template = unit.SourceCardTemplate;
                if (template != null)
                {
                    foreach (var effect in template.GetOnUnpairEffects())
                        effect.Resolve(unit, state, unit);
                }
                unit.PairingWithPartnerCard = false;
                GameVisualManager.Instance?.UpdatePartnerCardDraggable(unit.OwnerPlayerId);
            }
            else if (unit.IsPairedWithUnit)
            {
                var partner = unit.GetPairTargetUnitOrNull();
                if (partner != null && partner.IsPartner && partner.OwnerPlayerId == 0)
                    DialogueManager.Instance?.OnPartnerRemovedFromPairing(unit.SourceCardTemplate?.CardName ?? "", reason);
                // 表示更新（NotifyUnitAttackChanged）が GetEffectiveAttack でペア解除済みと見えるよう、先に参照を外す。
                unit.PairingTarget = null;
                partner.PairingTarget = null;
                ApplyAndClearPairingBonus(unit);
                ApplyAndClearPairingBonus(partner);
                var state = pm.GetGameStateForPlayer(unit.OwnerPlayerId);

                var partnerTemplate = partner.SourceCardTemplate;
                if (partnerTemplate != null)
                {
                    foreach (var effect in partnerTemplate.GetOnUnpairEffects())
                        effect.Resolve(unit, state, partner);
                }

                var myTemplate = unit.SourceCardTemplate;
                if (myTemplate != null)
                {
                    foreach (var effect in myTemplate.GetOnUnpairEffects())
                        effect.Resolve(unit, state, unit);
                }
            }

            if (needPartnerSweating)
            {
                var count = DialogueManager.Instance != null ? DialogueManager.Instance.PartnerPairingTargetCount : 0;
                StandingPictureManager.Instance?.SetStandingPicture(count == 0 ? StandingPictureType.Normal : StandingPictureType.Embarrassed);
            }

            pm.NotifyUnitDestroyed(unit, reason);
        }

        /// <summary>
        /// ペアリング結果を適用する（Unit 同士の相互参照・PartnerCard フラグ・効果 Resolve・通知・立ち絵）。
        /// </summary>
        public static void ApplyPairingResult(
            Unit unit,
            EffectTarget target,
            IReadOnlyList<IOnPairingEffect> effects,
            GameState state,
            PlayerData myData,
            PlayerData oppData)
        {
            Unit pairTargetUnit = null;
            if (target.Kind == EffectTargetKind.Unit && target.UnitInstanceId != null)
            {
                var b = myData.FieldZone.Units.Find(u => u.InstanceId == target.UnitInstanceId.Value)
                    ?? oppData.FieldZone.Units.Find(u => u.InstanceId == target.UnitInstanceId.Value);
                if (b != null)
                {
                    unit.PairingTarget = b;
                    b.PairingTarget = unit;
                    pairTargetUnit = b;
                }
            }
            else if (target.Kind == EffectTargetKind.PartnerCard && target.PlayerId is int ownerId)
            {
                unit.PairingWithPartnerCard = true;
                GameVisualManager.Instance?.UpdatePartnerCardDraggable(ownerId);
            }

            var isPartnerChosenAsTarget = (target.Kind == EffectTargetKind.PartnerCard && target.PlayerId == 0)
                || (pairTargetUnit != null && pairTargetUnit.IsPartner && pairTargetUnit.OwnerPlayerId == 0);
            if (isPartnerChosenAsTarget)
                DialogueManager.Instance?.OnPartnerChosenAsPairingTarget();

            if (effects != null)
            {
                foreach (var effect in effects)
                    effect.Resolve(target, state, unit, pairTargetUnit);
            }

            if (unit.SourceCardTemplate is ICopiesAttackFromPairTarget || unit.SourceCardTemplate is IAddsPairAttackToSelf)
                PlayerManager.Instance?.NotifyUnitAttackChanged(unit);

            if (unit.SourceCardTemplate is IPairingStandingPicture psp)
            {
                var typeId = psp.GetStandingPictureTypeWhenPartnerChosen(target, pairTargetUnit);
                if (!string.IsNullOrEmpty(typeId))
                    StandingPictureManager.Instance?.SetStandingPicture(typeId);
            }
        }

        /// <summary>
        /// パートナーがユニットとして召喚されたとき、PairingWithPartnerCard のユニットを新パートナーユニットと相互にペアし直す。
        /// </summary>
        public static void MigratePartnerCardPairingToUnit(Unit newPartnerUnit, FieldZone fieldOfOwner)
        {
            if (newPartnerUnit == null || fieldOfOwner?.Units == null) return;
            foreach (var u in fieldOfOwner.Units)
            {
                if (u != newPartnerUnit && u.PairingWithPartnerCard)
                {
                    u.PairingTarget = newPartnerUnit;
                    newPartnerUnit.PairingTarget = u;
                    u.PairingWithPartnerCard = false;
                    break;
                }
            }
        }
    }
}
