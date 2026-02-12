using System.Collections.Generic;
using CardBattle.AI;
using CardBattle.Core.Effects;
using CardBattle.Core.Field;
using UnityEngine;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// 相手フィールドのユニット1体を選んで1ダメージを与える呪文カード。
    /// </summary>
    public class DealOneDamageSpellCard : SpellCardTemplateBase, ISpellEffect
    {
        public DealOneDamageSpellCard()
        {
            cardName = "一撃";
            playCost = 1;
        }

        public IList<EffectTarget> GetAvailableTargets(GameState state)
        {
            return GetTargets(state);
        }

        public void Resolve(EffectTarget target, GameState state)
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
