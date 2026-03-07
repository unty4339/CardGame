using System.Collections.Generic;
using CardBattle.AI;
using CardBattle.Core.Effects;
using CardBattle.Core.Enums;
using CardBattle.Core.Field;
using CardBattle.Managers;
using CardBattle.UI;
using UnityEngine;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// グリンスキンの苗床。トーテム。発動時ペア・ペアリング中攻撃できない付与済み。ターン開始時：ゴブリン2枚手札に加え、これを破壊しペアリングを解除する。
    /// </summary>
    public class GrimskinNurseryTotemCard : TotemCardTemplateBase, IOnPairingEffect, IGrantsCannotAttackToPairTarget, IPairingStandingPicture, ITurnStartEffect
    {
        public GrimskinNurseryTotemCard()
        {
            cardName = "グリンスキンの苗床";
            playCost = 1;
            description = "トーテム\n発動時：\nペアリング中でない、自分パートナーか体力1の相手ユニットとペアリングする。\nペアリング中：\nペア対象に「攻撃できない」を付与する。\n自分ターン開始時：\n「ゴブリン」2枚を手札に加え、これを破壊しペアリングを解除する。";
            totemData = ScriptableObject.CreateInstance<TotemData>();
        }

        public IList<EffectTarget> GetAvailableTargets(GameState state, Unit sourceUnit, bool isPartnerOnField)
        {
            return PairingTargetCandidates.GetStandardPairingTargets(state, isPartnerOnField);
        }

        public void Resolve(EffectTarget target, GameState state, Unit sourceUnit, Unit pairTargetUnitOrNull)
        {
            if (pairTargetUnitOrNull != null)
            {
                pairTargetUnitOrNull.CanAttackUnit = false;
                pairTargetUnitOrNull.CanAttackPlayer = false;
            }
        }

        public string GetStandingPictureTypeWhenPartnerChosen(EffectTarget target, Unit pairTargetUnitOrNull)
        {
            var isPartnerChosen = target.Kind == EffectTargetKind.PartnerCard
                || (pairTargetUnitOrNull != null && pairTargetUnitOrNull.IsPartner && pairTargetUnitOrNull.OwnerPlayerId == 0);
            return isPartnerChosen ? StandingPictureType.Back : null;
        }

        /// <summary>
        /// ターン開始時：ゴブリン2枚手札に加え、このトーテムを破壊しペアリングを解除する。ペア対象は破壊しない。
        /// </summary>
        public void Resolve(Unit sourceUnit, int turnPlayerId)
        {
            if (sourceUnit == null || !sourceUnit.IsPaired) return;

            var playerManager = PlayerManager.Instance;
            if (playerManager == null) return;

            var data = playerManager.GetPlayerData(turnPlayerId);
            if (data == null) return;

            var goblin = new GoblinUnitCard();
            playerManager.AddCardToHand(turnPlayerId, goblin);
            playerManager.AddCardToHand(turnPlayerId, goblin);

            if (sourceUnit.PairingWithPartnerCard)
            {
                playerManager.UnpairPartnerCardOnly(sourceUnit);
            }

            playerManager.UnpairIfNeededAndNotifyDestroyed(sourceUnit, UnitDestroyReason.Nursery);
            data.FieldZone.Units.Remove(sourceUnit);
        }
    }
}
