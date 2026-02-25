using System.Collections.Generic;
using CardBattle.AI;
using CardBattle.Core.Effects;
using CardBattle.Core.Enums;
using CardBattle.Core.Field;
using CardBattle.UI;
using UnityEngine;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// グリンスキンの苗床。トーテム。発動時ペア・ペアリング中攻撃できない付与済み。ターン開始時等の効果はトーテム用フック拡張が必要。
    /// </summary>
    public class GrimskinNurseryTotemCard : TotemCardTemplateBase, IOnPairingEffect, IGrantsCannotAttackToPairTarget, IPairingStandingPicture
    {
        public GrimskinNurseryTotemCard()
        {
            cardName = "グリンスキンの苗床";
            playCost = 2;
            description = "トーテム\n\n発動時：\nペアリング中でない、自分パートナーか体力1の相手ユニットとペアリングする。\n\nペアリング中：\nペア対象に「攻撃できない」を付与する。\n\nターン開始時：\n「ゴブリン」を2枚手札に加え、ペア対象とこれを破壊する。\n手札を1枚捨てることで、これを破壊する代わりに手札に戻す。\n\nペアリング解除時：\nこのカードを破壊する。";
            totemData = ScriptableObject.CreateInstance<TotemData>();
        }

        public IList<EffectTarget> GetAvailableTargets(GameState state, Unit sourceUnit, bool isPartnerOnField)
        {
            var list = new List<EffectTarget>();

            if (state?.OpponentField?.Units != null)
            {
                foreach (var u in state.OpponentField.Units)
                {
                    if (u.HP == 1 && u.PairingTarget == null && !state.IsAlreadySomeonesPairingTarget(u))
                        list.Add(EffectTarget.Unit(u.InstanceId));
                }
            }

            if (state?.MyField?.Units != null && isPartnerOnField)
            {
                foreach (var u in state.MyField.Units)
                {
                    if (u.IsPartner && u.PairingTarget == null && !state.IsAlreadySomeonesPairingTarget(u))
                        list.Add(EffectTarget.Unit(u.InstanceId));
                }
            }

            if (!isPartnerOnField && state != null && !state.IsPartnerCardAlreadyPairingTarget())
                list.Add(EffectTarget.PartnerCard(state.MyPlayerId));

            return list;
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
    }
}
