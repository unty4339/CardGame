using System.Collections.Generic;
using CardBattle.AI;
using CardBattle.Core.Effects;
using CardBattle.Core.Enums;
using CardBattle.Core.Field;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// 緑光の主。超速攻、登場時相手ユニット1体破壊。
    /// </summary>
    public class GreenLightHeraldUnitCard : UnitCardTemplateBase, IOnSummonEffect
    {
        public GreenLightHeraldUnitCard()
        {
            cardName = "緑光の主";
            playCost = 8;
            baseHP = 6;
            baseAttack = 3;
            keywords.Add(KeywordAbility.DivineSpeed);
            description = "超速攻\n登場時：相手のユニットをひとつ選ぶ。そのユニットを破壊する。";
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
            var idx = state.OpponentField.Units.FindIndex(u => u.InstanceId == target.UnitInstanceId.Value);
            if (idx >= 0)
                state.OpponentField.Units.RemoveAt(idx);
        }
    }
}
