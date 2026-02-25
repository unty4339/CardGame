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
    /// グリンスキンの苗床。トーテム。発動時ペア・ペアリング中攻撃できない付与済み。ターン開始時：ゴブリン2枚手札に加え、ペア対象とこれを破壊。
    /// </summary>
    public class GrimskinNurseryTotemCard : TotemCardTemplateBase, IOnPairingEffect, IGrantsCannotAttackToPairTarget, IPairingStandingPicture, ITurnStartEffect
    {
        public GrimskinNurseryTotemCard()
        {
            cardName = "グリンスキンの苗床";
            playCost = 1;
            description = "トーテム\n発動時：\nペアリング中でない、自分パートナーか体力1の相手ユニットとペアリングする。\nペアリング中：\nペア対象に「攻撃できない」を付与する。\n自分ターン開始時：\n「ゴブリン」2枚を手札に加え、ペア対象とこれを破壊する。";
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
            return target.Kind == EffectTargetKind.PartnerCard ? StandingPictureType.Submission : null;
        }

        /// <summary>
        /// ターン開始時：ゴブリン2枚手札に加え、ペア対象とこのトーテムを破壊する。ペア対象がパートナーカードの場合はペアリング解除のみ行う。
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
            else
            {
                var pairTarget = sourceUnit.GetPairTargetUnitOrNull();
                if (pairTarget != null)
                {
                    var pairTargetOwnerData = playerManager.GetPlayerData(pairTarget.OwnerPlayerId);
                    playerManager.UnpairIfNeededAndNotifyDestroyed(pairTarget);
                    pairTargetOwnerData?.FieldZone.Units.Remove(pairTarget);
                }
            }

            playerManager.UnpairIfNeededAndNotifyDestroyed(sourceUnit);
            data.FieldZone.Units.Remove(sourceUnit);
        }
    }
}
