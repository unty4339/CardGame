using System.Collections.Generic;
using CardBattle.AI;
using CardBattle.Core.Effects;
using CardBattle.Core.Field;
using CardBattle.Managers;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// 騎士団長。5/4/4。登場時：相手ユニット1体に2ダメージを与える。
    /// </summary>
    public class KnightCommanderUnitCard : UnitCardTemplateBase, IOnSummonEffect
    {
        private const int DamageAmount = 2;

        public KnightCommanderUnitCard()
        {
            cardName = "騎士団長";
            playCost = 5;
            baseHP = 4;
            baseAttack = 4;
            description = "登場時、相手ユニット1体に2ダメージを与える。";
        }

        public IList<EffectTarget> GetAvailableTargets(GameState state, Unit sourceUnit)
        {
            var list = new List<EffectTarget>();
            if (state?.OpponentField == null) return list;
            foreach (var u in state.OpponentField.GetTargetableUnits())
            {
                list.Add(EffectTarget.Unit(u.InstanceId));
            }
            return list;
        }

        public void Resolve(EffectTarget target, GameState state, Unit sourceUnit)
        {
            ApplyDamage(target, state);
        }

        private static void ApplyDamage(EffectTarget target, GameState state)
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
