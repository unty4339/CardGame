using System.Collections.Generic;
using CardBattle.AI;
using CardBattle.Core.Effects;
using CardBattle.Core.Field;
using CardBattle.Managers;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// 騎士団の突撃。コスト2。相手ユニット1体に2ダメージを与える。
    /// </summary>
    public class KnightChargeSpellCard : SpellCardTemplateBase, ISpellEffect
    {
        private const int DamageAmount = 2;

        public KnightChargeSpellCard()
        {
            cardName = "騎士団の突撃";
            playCost = 2;
            description = "相手ユニット1体に2ダメージを与える。";
        }

        public IList<EffectTarget> GetAvailableTargets(GameState state)
        {
            var list = new List<EffectTarget>();
            if (state?.OpponentField == null) return list;
            foreach (var u in state.OpponentField.GetTargetableUnits())
            {
                list.Add(EffectTarget.Unit(u.InstanceId));
            }
            return list;
        }

        public void Resolve(EffectTarget target, GameState state)
        {
            if (state?.OpponentField?.Units == null) return;
            if (target.Kind != EffectTargetKind.Unit || target.UnitInstanceId == null) return;
            var unit = state.OpponentField.Units.Find(u => u.InstanceId == target.UnitInstanceId.Value);
            if (unit == null || unit.IsTotem) return;
            PlayerManager.Instance?.AddUnitHp(unit, -DamageAmount);
            if (unit.HP <= 0)
                state.OpponentField.Units.Remove(unit);
        }
    }
}
