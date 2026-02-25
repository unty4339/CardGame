using System.Collections.Generic;
using CardBattle.AI;
using CardBattle.Core.Effects;
using CardBattle.Core.Field;
using CardBattle.Managers;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// 騎士団の弓手。2/1/2。登場時：相手ユニット1体に1ダメージを与える。
    /// </summary>
    public class KnightArcherUnitCard : UnitCardTemplateBase, IOnSummonEffect
    {
        public KnightArcherUnitCard()
        {
            cardName = "騎士団の弓手";
            playCost = 2;
            baseHP = 1;
            baseAttack = 2;
            description = "登場時、相手ユニット1体に1ダメージを与える。";
        }

        public IList<EffectTarget> GetAvailableTargets(GameState state, Unit sourceUnit)
        {
            var list = new List<EffectTarget>();
            if (state?.OpponentField?.Units == null) return list;
            foreach (var u in state.OpponentField.Units)
            {
                if (u.IsTotem) continue;
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
            PlayerManager.Instance?.AddUnitHp(unit, -1);
            if (unit.HP <= 0)
                state.OpponentField.Units.Remove(unit);
        }
    }
}
