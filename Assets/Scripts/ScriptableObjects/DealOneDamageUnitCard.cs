using System.Collections.Generic;
using CardBattle.AI;
using CardBattle.Core.Effects;
using CardBattle.Core.Field;
using UnityEngine;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// 召喚時：相手フィールドのユニット1体を選んで1ダメージを与えるユニットカード。
    /// </summary>
    public class DealOneDamageUnitCard : UnitCardTemplateBase, IOnSummonEffect
    {
        public DealOneDamageUnitCard()
        {
            cardName = "召喚の一撃";
            playCost = 1;
            baseHP = 1;
            baseAttack = 1;
        }

        public IList<EffectTarget> GetAvailableTargets(GameState state, Unit sourceUnit)
        {
            return GetTargets(state);
        }

        public void Resolve(EffectTarget target, GameState state, Unit sourceUnit)
        {
            ApplyDamage(target, state);
        }

        private static IList<EffectTarget> GetTargets(GameState state)
        {
            var list = new List<EffectTarget>();
            if (state?.OpponentField?.Units == null) return list;
            foreach (var u in state.OpponentField.Units)
                list.Add(EffectTarget.Unit(u.InstanceId));
            return list;
        }

        private static void ApplyDamage(EffectTarget target, GameState state)
        {
            if (state?.OpponentField?.Units == null) return;
            if (target.Kind != EffectTargetKind.Unit || target.UnitInstanceId == null) return;
            var unit = state.OpponentField.Units.Find(u => u.InstanceId == target.UnitInstanceId.Value);
            if (unit == null) return;
            unit.HP -= 1;
            if (unit.HP <= 0)
                state.OpponentField.Units.Remove(unit);
        }
    }
}
