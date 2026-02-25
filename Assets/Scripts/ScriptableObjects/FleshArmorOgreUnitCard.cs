using System.Collections.Generic;
using CardBattle.AI;
using CardBattle.Core.Effects;
using CardBattle.Core.Field;
using CardBattle.Managers;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// 肉鎧のオーク。登場時ペア、ペアリング時+X/+X、ペアリング中攻撃できない付与済み、破壊時ペアを代わりに破壊。
    /// 戦闘破壊肩代わりは拡張が必要。ペアリング対象選択と+X/+Xは実装済み。
    /// </summary>
    public class FleshArmorOgreUnitCard : UnitCardTemplateBase, IOnPairingEffect, IGrantsCannotAttackToPairTarget
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
            var list = new List<EffectTarget>();
            if (state?.OpponentField?.Units != null)
            {
                foreach (var u in state.OpponentField.Units)
                {
                    if (u.HP == 1)
                        list.Add(EffectTarget.Unit(u.InstanceId));
                }
            }
            if (state?.MyField?.Units != null && isPartnerOnField)
            {
                foreach (var u in state.MyField.Units)
                {
                    if (u.IsPartner)
                        list.Add(EffectTarget.Unit(u.InstanceId));
                }
            }
            if (!isPartnerOnField && state != null)
                list.Add(EffectTarget.PartnerCard(state.MyPlayerId));
            return list;
        }

        public void Resolve(EffectTarget target, GameState state, Unit sourceUnit, Unit pairTargetUnitOrNull)
        {
            if (sourceUnit == null || pairTargetUnitOrNull == null) return;
            var x = pairTargetUnitOrNull.Attack;
            PlayerManager.Instance?.AddUnitAttack(sourceUnit, x);
            PlayerManager.Instance?.AddUnitHp(sourceUnit, x);
            pairTargetUnitOrNull.CanAttackUnit = false;
            pairTargetUnitOrNull.CanAttackPlayer = false;
        }
    }
}
