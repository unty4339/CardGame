using System.Collections.Generic;
using CardBattle.AI;
using CardBattle.Core.Effects;
using CardBattle.Core.Field;
using CardBattle.Managers;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// グリンスキンの緑光。相手ユニット1体に体力が1残るように最大3ダメージ与える。
    /// </summary>
    public class GrimskinGreenLightSpellCard : SpellCardTemplateBase, ISpellEffect
    {
        public GrimskinGreenLightSpellCard()
        {
            cardName = "グリンスキンの緑光";
            playCost = 2;
            description = "スペル\n発動時：\n相手のユニットを1体選ぶ。体力が1残るように、最大で3ダメージ与える。";
        }

        public IList<EffectTarget> GetAvailableTargets(GameState state)
        {
            var list = new List<EffectTarget>();
            if (state?.OpponentField?.Units == null) return list;
            foreach (var u in state.OpponentField.Units)
                list.Add(EffectTarget.Unit(u.InstanceId));
            return list;
        }

        public void Resolve(EffectTarget target, GameState state)
        {
            if (state?.OpponentField?.Units == null) return;
            if (target.Kind != EffectTargetKind.Unit || target.UnitInstanceId == null) return;
            var unit = state.OpponentField.Units.Find(u => u.InstanceId == target.UnitInstanceId.Value);
            if (unit == null) return;

            var damage = System.Math.Min(3, unit.HP - 1);
            if (damage > 0)
                PlayerManager.Instance?.AddUnitHp(unit, -damage);
        }
    }
}
