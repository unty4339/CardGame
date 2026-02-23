using System.Collections.Generic;
using CardBattle.AI;
using CardBattle.Core.Effects;
using CardBattle.Core.Field;
using UnityEngine;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// グリンスキンの苗床。トーテム。発動時ペア・ターン開始時等の効果はトーテム用フック拡張が必要。
    /// </summary>
    public class GrimskinNurseryTotemCard : TotemCardTemplateBase, IOnPairingEffect
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
                    if (u.HP == 1 && u.PairingTarget == null)
                        list.Add(EffectTarget.Unit(u.InstanceId));
                }
            }

            if (state?.MyField?.Units != null && isPartnerOnField)
            {
                foreach (var u in state.MyField.Units)
                {
                    if (u.IsPartner && u.PairingTarget == null)
                        list.Add(EffectTarget.Unit(u.InstanceId));
                }
            }

            if (!isPartnerOnField && state != null)
                list.Add(EffectTarget.PartnerCard(state.MyPlayerId));

            return list;
        }

        public void Resolve(EffectTarget target, GameState state, Unit sourceUnit, Unit pairTargetUnitOrNull)
        {
            // ペアリング時の見た目は UnitManager.ApplyPairingResult の StandingPicture 分岐で処理される
        }
    }
}
