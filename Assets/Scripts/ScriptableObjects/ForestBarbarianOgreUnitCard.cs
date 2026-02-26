using System.Collections.Generic;
using CardBattle.AI;
using CardBattle.Core.Effects;
using CardBattle.Core.Field;
using CardBattle.Managers;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// 森の蛮人、オーク。登場時：相手のユニットを1体選び、その体力を1にする。
    /// </summary>
    public class ForestBarbarianOgreUnitCard : UnitCardTemplateBase, IOnSummonEffect
    {
        public ForestBarbarianOgreUnitCard()
        {
            cardName = "森の蛮人、オーク";
            playCost = 5;
            baseHP = 4;
            baseAttack = 4;
            description = "登場時：\n相手のユニットを1体選ぶ。そのユニットの体力を1にする。";
        }

        public IList<EffectTarget> GetAvailableTargets(GameState state, Unit sourceUnit)
        {
            var list = new List<EffectTarget>();
            if (state?.OpponentField?.Units == null) return list;
            foreach (var u in state.OpponentField.Units)
                list.Add(EffectTarget.Unit(u.InstanceId));
            return list;
        }

        public void Resolve(EffectTarget target, GameState state, Unit sourceUnit)
        {
            if (state?.OpponentField?.Units == null) return;
            if (target.Kind != EffectTargetKind.Unit || target.UnitInstanceId == null) return;
            var unit = state.OpponentField.Units.Find(u => u.InstanceId == target.UnitInstanceId.Value);
            if (unit != null)
                PlayerManager.Instance?.SetUnitHp(unit, 1);
        }
    }
}
